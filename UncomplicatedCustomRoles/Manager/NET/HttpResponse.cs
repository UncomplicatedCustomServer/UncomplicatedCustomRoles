/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Net;

namespace UncomplicatedCustomRoles.Manager.NET;

internal readonly struct HttpResponse
{
    internal HttpResponse(long statusCode, string body, string error)
    {
        StatusCode = statusCode;
        Body = body;
        Error = error;
    }

    public long StatusCode { get; }

    public string Body { get; }

    public string Error { get; }

    public bool Completed => StatusCode > 0;

    public bool IsSuccess => StatusCode is >= 200 and < 300;

    public HttpStatusCode Status => Completed ? (HttpStatusCode)StatusCode : HttpStatusCode.ServiceUnavailable;

    public string Reason => Error ?? (Completed ? $"HTTP {StatusCode}" : "the server did not answer");
}