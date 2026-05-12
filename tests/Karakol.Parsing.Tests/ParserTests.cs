using System.Runtime.CompilerServices;
using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Sources;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Parsing.Parsers.AccessLogs;
using Karakol.Parsing.Parsers.AuthLogs;
using Karakol.Parsing.Parsers.Generic;
using Karakol.Parsing.Parsers.Structured;
using Karakol.Parsing.Results;

namespace Karakol.Parsing.Tests;

public sealed class ParserTests
{
    [Fact]
    public async Task Nginx_parser_parses_combined_access_log()
    {
        var result = await ParseSingleAsync(new NginxAccessLogParser(), "127.0.0.1 - - [10/Oct/2026:13:55:36 +0300] \"GET /login?id=1 HTTP/1.1\" 200 2326 \"-\" \"Mozilla/5.0\"");

        result.IsSuccess.Should().BeTrue();
        result.Event!.SourceIp.Should().Be("127.0.0.1");
        result.Event.Path.Should().Be("/login");
        result.Event.QueryString.Should().Be("id=1");
        result.Event.UserAgent.Should().Be("Mozilla/5.0");
    }

    [Fact]
    public async Task Nginx_parser_returns_error_for_invalid_line()
    {
        var result = await ParseSingleAsync(new NginxAccessLogParser(), "not nginx");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task Apache_parser_parses_common_access_log()
    {
        var result = await ParseSingleAsync(new ApacheAccessLogParser(), "127.0.0.1 - - [10/Oct/2026:13:55:36 +0300] \"GET /home HTTP/1.1\" 200 2326");

        result.IsSuccess.Should().BeTrue();
        result.Event!.Path.Should().Be("/home");
        result.Event.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Ssh_parser_parses_failed_password()
    {
        var result = await ParseSingleAsync(new SshAuthLogParser(), "Jan 12 10:15:32 server sshd[12345]: Failed password for invalid user admin from 192.168.1.20 port 54321 ssh2");

        result.IsSuccess.Should().BeTrue();
        result.Event!.EventType.Should().Be("failed_login");
        result.Event.Username.Should().Be("admin");
        result.Event.SourceIp.Should().Be("192.168.1.20");
    }

    [Fact]
    public async Task Ssh_parser_parses_accepted_password()
    {
        var result = await ParseSingleAsync(new SshAuthLogParser(), "Jan 12 10:15:40 server sshd[12347]: Accepted password for deploy from 10.0.0.5 port 55555 ssh2");

        result.IsSuccess.Should().BeTrue();
        result.Event!.EventType.Should().Be("accepted_login");
        result.Event.Username.Should().Be("deploy");
    }

    [Fact]
    public async Task Auth_parser_reuses_ssh_auth_extraction_with_auth_format()
    {
        var result = await ParseSingleAsync(new AuthLogParser(), "Jan 12 10:15:32 server sshd[12345]: Failed password for invalid user admin from 192.168.1.20 port 54321 ssh2");

        result.IsSuccess.Should().BeTrue();
        result.Event!.LogFormat.Should().Be(LogFormat.Auth);
        result.Event.EventType.Should().Be("failed_login");
    }

    [Fact]
    public async Task Json_parser_maps_common_fields()
    {
        var result = await ParseSingleAsync(new JsonSecurityEventParser(), """{"sourceIp":"1.2.3.4","method":"GET","url":"/search?q=test","status":400,"userAgent":"agent"}""");

        result.IsSuccess.Should().BeTrue();
        result.Event!.SourceIp.Should().Be("1.2.3.4");
        result.Event.Path.Should().Be("/search");
        result.Event.QueryString.Should().Be("q=test");
    }

    [Fact]
    public async Task Json_parser_returns_error_for_invalid_json()
    {
        var result = await ParseSingleAsync(new JsonSecurityEventParser(), "{broken");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Json_parser_supports_single_line_array()
    {
        var results = await ParseAsync(new JsonSecurityEventParser(), """[{"source_ip":"1.2.3.4","url":"/a","status_code":200},{"source_ip":"5.6.7.8","url":"/b?q=1","status_code":404}]""");

        results.Should().HaveCount(2);
        results[0].Event!.SourceIp.Should().Be("1.2.3.4");
        results[1].Event!.Path.Should().Be("/b");
        results[1].Event!.QueryString.Should().Be("q=1");
    }

    [Fact]
    public async Task Json_parser_supports_multiline_array()
    {
        var results = await ParseAsync(
            new JsonSecurityEventParser(),
            """[""",
            """{"source_ip":"1.2.3.4","url":"/a","status_code":200},""",
            """{"source_ip":"5.6.7.8","url":"/b?q=1","status_code":404}""",
            """]""");

        results.Should().HaveCount(2);
        results[0].Event!.SourceIp.Should().Be("1.2.3.4");
        results[1].Event!.Path.Should().Be("/b");
    }

    [Fact]
    public async Task Json_parser_allows_missing_optional_fields_and_keeps_unknown_metadata()
    {
        var result = await ParseSingleAsync(new JsonSecurityEventParser(), """{"message":"login failed","tenant":"demo","trace_id":"abc-123"}""");

        result.IsSuccess.Should().BeTrue();
        result.Event!.SourceIp.Should().BeNull();
        result.Event.StatusCode.Should().BeNull();
        result.Event.Metadata.Should().Contain("tenant", "demo");
        result.Event.Metadata.Should().Contain("trace_id", "abc-123");
    }

    [Fact]
    public async Task Csv_parser_maps_header_fields()
    {
        var parser = new CsvSecurityEventParser();
        var results = await ParseAsync(parser, "sourceIp,method,url,status,userAgent", "1.2.3.4,GET,/login?id=1,401,curl");

        results.Should().ContainSingle();
        results[0].Event!.SourceIp.Should().Be("1.2.3.4");
        results[0].Event!.Path.Should().Be("/login");
    }

    [Fact]
    public async Task Csv_parser_handles_quoted_commas()
    {
        var parser = new CsvSecurityEventParser();
        var results = await ParseAsync(parser, "source_ip,url,status_code,user_agent", "\"1.2.3.4\",\"/search?q=a,b\",400,\"Mozilla, Test\"");

        results.Should().ContainSingle();
        results[0].Event!.SourceIp.Should().Be("1.2.3.4");
        results[0].Event!.QueryString.Should().Be("q=a,b");
        results[0].Event!.UserAgent.Should().Be("Mozilla, Test");
    }

    [Fact]
    public async Task Csv_parser_supports_column_mapping_options()
    {
        var parser = new CsvSecurityEventParser();
        var source = new LogSource(LogSourceId.New(), "test.csv", parser.Format, 0, null, null, LogSourceType.File);
        var options = new ParserOptions(source, new Dictionary<string, string>
        {
            ["client"] = "source_ip",
            ["request"] = "url",
            ["code"] = "status_code"
        });
        var results = new List<ParseResult>();
        await foreach (var result in parser.ParseAsync(ToLines(["client,request,code", "8.8.8.8,/admin,404"]), options, CancellationToken.None))
        {
            if (result.LineNumber > 1)
            {
                results.Add(result);
            }
        }

        results.Should().ContainSingle();
        results[0].Event!.SourceIp.Should().Be("8.8.8.8");
        results[0].Event!.Path.Should().Be("/admin");
        results[0].Event!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Csv_parser_keeps_unknown_columns_as_metadata()
    {
        var parser = new CsvSecurityEventParser();
        var results = await ParseAsync(parser, "source_ip,url,status_code,tenant,trace_id", "1.2.3.4,/login,401,demo,abc-123");

        results.Should().ContainSingle();
        var securityEvent = results[0].Event!;
        securityEvent.Metadata.Should().Contain("tenant", "demo");
        securityEvent.Metadata.Should().Contain("trace_id", "abc-123");
    }

    [Fact]
    public async Task Access_log_parser_accepts_missing_protocol_in_request()
    {
        var result = await ParseSingleAsync(new NginxAccessLogParser(), "127.0.0.1 - - [10/Oct/2026:13:55:36 +0300] \"GET /health\" 200 - \"-\" \"curl/8.0\"");

        result.IsSuccess.Should().BeTrue();
        result.Event!.Path.Should().Be("/health");
        result.Event.Protocol.Should().BeNull();
        result.Event.ResponseSize.Should().BeNull();
    }

    [Fact]
    public async Task Access_log_parser_parses_ipv6_source_ip()
    {
        var result = await ParseSingleAsync(new NginxAccessLogParser(), "2001:db8::10 - - [10/Oct/2026:13:55:36 +0300] \"GET /ipv6 HTTP/1.1\" 200 128 \"-\" \"Mozilla/5.0\"");

        result.IsSuccess.Should().BeTrue();
        result.Event!.SourceIp.Should().Be("2001:db8::10");
        result.Event.Path.Should().Be("/ipv6");
    }

    [Fact]
    public async Task Access_log_parser_accepts_missing_referrer_and_user_agent()
    {
        var result = await ParseSingleAsync(new ApacheAccessLogParser(), "127.0.0.1 - - [10/Oct/2026:13:55:37 +0300] \"GET /minimal HTTP/1.1\" 204 -");

        result.IsSuccess.Should().BeTrue();
        result.Event!.Referrer.Should().BeNull();
        result.Event.UserAgent.Should().BeNull();
        result.Event.ResponseSize.Should().BeNull();
    }

    [Fact]
    public async Task Access_log_parser_handles_escaped_quote_inside_url()
    {
        var result = await ParseSingleAsync(new NginxAccessLogParser(), "127.0.0.1 - - [10/Oct/2026:13:55:39 +0300] \"GET /escaped?q=\\\"quoted\\\" HTTP/1.1\" 200 64 \"-\" \"Mozilla/5.0\"");

        result.IsSuccess.Should().BeTrue();
        result.Event!.Path.Should().Be("/escaped");
        result.Event.QueryString.Should().Be("q=\"quoted\"");
    }

    [Fact]
    public async Task Access_log_parser_returns_error_for_malformed_timestamp()
    {
        var result = await ParseSingleAsync(new NginxAccessLogParser(), "127.0.0.1 - - [broken] \"GET /bad-time HTTP/1.1\" 200 64 \"-\" \"Mozilla/5.0\"");

        result.IsSuccess.Should().BeFalse();
        result.Error!.Reason.Should().Contain("timestamp");
    }

    [Fact]
    public async Task Generic_parser_extracts_source_ip()
    {
        var result = await ParseSingleAsync(new GenericRegexLogParser(), "connection from 8.8.8.8 failed");

        result.IsSuccess.Should().BeTrue();
        result.Event!.SourceIp.Should().Be("8.8.8.8");
    }

    private static async Task<ParseResult> ParseSingleAsync(Karakol.Parsing.Abstractions.ILogParser parser, string line)
    {
        var results = await ParseAsync(parser, line);
        return results.Single();
    }

    private static async Task<List<ParseResult>> ParseAsync(Karakol.Parsing.Abstractions.ILogParser parser, params string[] lines)
    {
        var source = new LogSource(LogSourceId.New(), "test.log", parser.Format, 0, null, null, LogSourceType.File);
        var results = new List<ParseResult>();
        await foreach (var result in parser.ParseAsync(ToLines(lines), new ParserOptions(source), CancellationToken.None))
        {
            if (result.LineNumber > 1 || parser is not CsvSecurityEventParser)
            {
                results.Add(result);
            }
        }

        return results;
    }

    private static async IAsyncEnumerable<LogLine> ToLines(string[] lines, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var index = 0; index < lines.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return new LogLine(index + 1, lines[index]);
        }
    }
}
