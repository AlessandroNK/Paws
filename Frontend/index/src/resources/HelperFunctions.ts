import {ApiError, Components, FetchOptions, Result} from "../types/CommonTypes.ts";
import {MessageDuration, MessageMood, MessageType, UiMessage} from "../types/MessageTypes.ts";

// ---------------------------------------------------------------------------------------------------------------------
/**
 * Creates the url to send the request basen on the environment
 * @param accessPoint
 * @return {string} The url to send the request to
 * @description This function checks if the app is running on localhost or a specific IP address.
 * If it is, it returns a local URL; otherwise, it returns the production URL.
 * The local URL is set to `http://localhost:5074/api/${accessPoint}`
 * and the production URL is set to `https://koodi-fbhcf8gtfgd5g7cd.canadacentral-01.azurewebsites.net/api/${accessPoint}`
 * @example
 * // If the app is running on localhost, it will return:
 * CreateUrlRequest('users'); // returns 'http://localhost:5074/api/users'
 * // If the app is running on production, it will return:
 * CreateUrlRequest('users'); // returns 'https://koodi-fbhcf8gtfgd5g7cd.canadacentral-01.azurewebsites.net/api/users'
 */
export function CreateUrlRequest(accessPoint: string): string {
    // TODO handle bad API directions
    const isLocal = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.id1";
    return isLocal
        ? `http://localhost:5243/api/${accessPoint}`
        : `https://koodi-fbhcf8gtfgd5g7cd.canadacentral-01.azurewebsites.net/api/${accessPoint}`;
}

// ---------------------------------------------------------------------------------------------------------------------
/**
 * Generate or retrieve a persistent Id
 */
export function GetDeviceId(): Result<string> {
    try {
        let id = localStorage.getItem('paws_device_id');
        if (!id) {
            id = crypto.randomUUID();
            localStorage.setItem('paws_device_id', id);
        }

        return Result.ok(id);
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<string>(
                `Error generating or retrieving device Id, error: ${error.message}`,
                500,
                Components.HELPER_FUNCTIONS,
                "DEVICE_ID_ERROR"
            ).log();
    }
}

// ---------------------------------------------------------------------------------------------------------------------
/**
 * Executes a fetch operation with a timeout. If the fetch does not complete within the specified timeout, it will be aborted.
 * @param {string} url - The url to send the fetch.
 * @param {Object} [options={}] - The options for the fetch request, such as method, headers, body, etc.
 * @param {number} timeout - The timeout duration in milliseconds (default is 60000 ms or 60 seconds).
 * @returns {Result} The result of the operation, could contain any data.
 * @throws {Error} If the operation times out.
 */
export async function ExecuteFetchWithTimeout(
    url: string,
    options: FetchOptions,
    timeout: number = 30000
): Promise<Result<unknown>> {

    // Create AbortController for timeout
    const controller = new AbortController();
    const id = setTimeout(() => controller.abort(), timeout);

    try {
        // Add signal to options
        options.signal = controller.signal;

        // Send the request
        const response = await fetch(url, options);
        clearTimeout(id);

        // Get JSON response
        const json = await response.json();
        const result = new Result(true, Components.API_RESPONSE_PROCESSING);

        // Set result properties based on response
        if (json.kind) {
            result.success = json.success;
            result.code = json.code;
            result.status = json.status;
            result.title = json.title;
            result.data = json.data;
        } else {
            result.success = response.ok;
            result.status = response.status;
            result.title = json.title || json.message;

            // Add local error
            result.addError(ApiCodeToUiMessage(response.status));
        }

        // Extract errors if present
        if (json.errors && typeof json.errors === "object") {
            for (const [field, messages] of Object.entries(json.errors)) {
                if (!Array.isArray(messages)) continue;

                const error = new ApiError(field ? field : "Validation error");
                for (const message of messages) error.addMessage(message);
                result.addError(error);
            }
        }

        return result.log();
    } catch (err) {
        clearTimeout(id);
        const error = err as Error;

        if (error.name === 'AbortError')
            return Result
                .fail<unknown>(
                    `Request to ${url} timed out after ${timeout} ms.`,
                    504,
                    Components.HELPER_FUNCTIONS,
                    "TIMEOUT_ERROR"
                ).log();

        return Result
            .fail<unknown>(
                `Network error while fetching ${url}, error: ${error.message}`,
                500,
                Components.HELPER_FUNCTIONS,
                "NETWORK_ERROR"
            ).log();
    }
}

// ---------------------------------------------------------------------------------------------------------------------
/**
 * Converts an API status code into a user-friendly message encapsulated in a Result object.
 * The function maps common HTTP status codes to predefined messages and moods, which can be used to inform the user
 * about the outcome of an API request.
 * @param status - The HTTP status code returned by the API.
 * @returns A Result object containing a UiMessage with the appropriate message, mood, and other details based on the status code.
 * @description This function takes an HTTP status code as input and creates a UiMessage object that includes a user-friendly message,
 * the mood associated with the status (e.g., happy for success, sad for client errors, etc.), and other relevant details.
 * It then wraps this UiMessage in a Result object, which indicates whether the operation was successful based on the status code.
 * The function handles a range of common HTTP status codes, providing specific messages for each, and defaults to a generic error message for unexpected codes.
 * @example
 * // For a successful response:
 * ApiCodeToUiMessage(200); // returns a Result with a UiMessage indicating "OK: The request has succeeded." and a happy mood.
 * // For a client error:
 * ApiCodeToUiMessage(404); // returns a Result with a UiMessage indicating "Not Found: The server can not find the requested resource." and a sad mood.
 * // For an unexpected status code:
 * ApiCodeToUiMessage(999); // returns a Result with a UiMessage indicating "Unexpected Error: 999" and a sad mood.
 */
function ApiCodeToUiMessage(status: number): ApiError {
    const uiMessage = new UiMessage();
    uiMessage.status = status;
    uiMessage.code = status.toString();
    uiMessage.type = MessageType.ERROR;
    uiMessage.component = Components.HTTP_SERVICE;
    uiMessage.duration = MessageDuration.MEDIUM;
    uiMessage.dismissible = true;

    switch (status) {
        case 200:
            uiMessage.message = "OK: The request has succeeded.";
            uiMessage.mood = MessageMood.HAPPY;
            break;
        case 201:
            uiMessage.message = "Created: The request has succeeded and a new resource has been created as a result.";
            uiMessage.mood = MessageMood.HAPPY;
            break;
        case 202:
            uiMessage.message = "Accepted: The request has been received but not yet acted upon.";
            uiMessage.mood = MessageMood.HAPPY;
            break;
        case 204:
            uiMessage.message = "No Content: The server successfully processed the request and is not returning any content.";
            uiMessage.mood = MessageMood.HAPPY;
            break;
        case 304:
            uiMessage.message = "Not Modified: The resource has not been modified since the last request.";
            uiMessage.mood = MessageMood.HAPPY;
            break;
        case 400:
            uiMessage.message = "Bad Request: The server could not understand the request due to invalid syntax.";
            uiMessage.mood = MessageMood.SAD;
            break;
        case 401:
            uiMessage.message = "Unauthorized: The client must authenticate itself to get the requested response.";
            uiMessage.mood = MessageMood.ANGRY;
            break;
        case 403:
            uiMessage.message = "Forbidden: The client does not have access rights to the content; that is, it is unauthorized.";
            uiMessage.mood = MessageMood.HAPPY;
            break;
        case 404:
            uiMessage.message = "Not Found: The server can not find the requested resource.";
            uiMessage.mood = MessageMood.HAPPY;
            break;
        case 422:
            uiMessage.message = "Unprocessable Entity: The request was well-formed but was unable to be followed due to semantic errors.";
            uiMessage.mood = MessageMood.SAD;
            break;
        case 500:
            uiMessage.message = "Internal Server Error: The server has encountered a situation it doesn't know how to handle.";
            uiMessage.mood = MessageMood.CRAZY;
            break;
        case 502:
            uiMessage.message = "Bad Gateway: The server was acting as a gateway or proxy and received an invalid response from the upstream server.";
            uiMessage.mood = MessageMood.SAD;
            break;
        case 503:
            uiMessage.message = "Service Unavailable: The server is not ready to handle the request, often due to maintenance or overload.";
            uiMessage.mood = MessageMood.SAD;
            break;
        case 504:
            uiMessage.message = "Gateway Timeout: The server was acting as a gateway or proxy and did not receive a timely response from the upstream server.";
            uiMessage.mood = MessageMood.SLEEPY;
            break;
        default:
            uiMessage.message = `Unexpected Error: ${status}`;
            uiMessage.mood = MessageMood.SAD;
    }

    const error = new ApiError("API Error");
    return error.addUiMessage(uiMessage);
}