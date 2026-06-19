// TODO convert services into classes and use dependency injection
import {Components, FetchOptions, Result} from "../types/CommonTypes.ts";
import {Appointment, User} from "../types/SystemTypes.ts";
import * as HelperFunctions from "../resources/HelperFunctions.ts";


//                                                                                                             FUNCTIONS
// ---------------------------------------------------------------------------------------------------------------------
export async function getUserFromSessionAsync(): Promise<Result<User | null>> {
    try {
        // Find local user session
        const sessionResult = HelperFunctions.getLocalUser();
        if (!sessionResult.success) return sessionResult;

        // Check on the API for active sessions
        // API request
        const deviceIdResult = HelperFunctions.GetDeviceId();
        if (!deviceIdResult.success || !deviceIdResult.data)
            return deviceIdResult.convertTo<null>();

        const accessPoint = "user/check-user-state";
        const url = HelperFunctions.CreateUrlRequest(accessPoint);
        const options: FetchOptions = {
            method: "POST",
            headers: {
                "Device-Id": deviceIdResult.data,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
        const result = await HelperFunctions.ExecuteFetchWithTimeout(url, options);
        if (result.code) result.component = Components.APPOINTMENT_SERVICE;
        if (!result.success || !result.data) return result.convertTo<null>();
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<null>(
                `Error fetching user from session, error: ${error.message}`,
                500,
                Components.USER_SERVICE,
                "USER_SESSION_FETCH_ERROR"
            ).log();
    }
}