// TODO convert services into classes and use dependency injection
import {Components, FetchOptions, Result} from "../types/CommonTypes.ts";
import {Pet, User} from "../types/SystemTypes.ts";
import * as HelperFunctions from "../resources/HelperFunctions.ts";
import {LoginRequest, StartLoginRequest} from "../types/RequestTypes.ts";

//                                                                                                             FUNCTIONS
// ---------------------------------------------------------------------------------------------------------------------
export function getLocalSession(): Result<User> {
    try {
        // Find local user session
        return HelperFunctions.getLocalUser();
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<User>(
                `Error fetching user from session, error: ${error.message}`,
                500,
                Components.USER_SERVICE,
                "USER_SESSION_FETCH_ERROR"
            ).log();
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export async function getApiSessionAsync(user: User): Promise<Result<User>> {
    try {
        // API request
        const deviceIdResult = HelperFunctions.getDeviceId();
        if (!deviceIdResult.success || !deviceIdResult.data)
            return deviceIdResult.convertTo<User>();

        const tokenResult = HelperFunctions.getSessionToken();
        if (!tokenResult.success || !tokenResult.data)
            return Result.fail<User>(
                "No session token found for the user",
                401,
                Components.USER_SERVICE,
                "USER_SESSION_TOKEN_MISSING"
            ).log();

        const accessPoint = "user/validate-session-token";
        const url = HelperFunctions.createUrlRequest(accessPoint);
        const options: FetchOptions = {
            method: "GET",
            headers: {
                "Device-Id": deviceIdResult.data,
                "Session-Token": tokenResult.data,
                "Content-Type": "application/json"
            },
        }
        const result = await HelperFunctions.executeFetchWithTimeout(url, options);
        if (result.code) result.component = Components.USER_SERVICE;
        if (!result.success || !result.data) return result.convertTo<User>();

        // Process response
        const responseData = result.data;
        user = new User(
            responseData.id,
            responseData.name,
            responseData.email,
            responseData.documentType,
            responseData.documentNumber
        );
        user.pets = responseData.pets.map((petData: object) => new Pet(
            petData.id,
            petData.name,
            petData.species,
            petData.breed,
            petData.age,
            petData.ownerId
        ));
        HelperFunctions.updateSessionToken(responseData.sessionToken);
        return Result.ok(user);
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<User>(
                `Error fetching user from session, error: ${error.message}`,
                500,
                Components.USER_SERVICE,
                "USER_SESSION_FETCH_ERROR"
            ).log();
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export async function startLoginProcessAsync(request: StartLoginRequest): Promise<Result<void>> {
    try {
        // API request
        const deviceIdResult = HelperFunctions.getDeviceId();
        if (!deviceIdResult.success || !deviceIdResult.data)
            return deviceIdResult.convertTo<void>();

        const accessPoint = "user/start-login-process";
        const url = HelperFunctions.createUrlRequest(accessPoint);
        const options: FetchOptions = {
            method: "POST",
            headers: {
                "Device-Id": deviceIdResult.data,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
        const result = await HelperFunctions.executeFetchWithTimeout(url, options);
        if (result.code) result.component = Components.USER_SERVICE;
        return result.convertTo<void>();
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<void>(
                `Error during login, error: ${error.message}`,
                500,
                Components.USER_SERVICE,
                "USER_LOGIN_ERROR"
            ).log();
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export async function loginWithCodeRequest(request: LoginRequest): Promise<Result<User>> {
    try {
        // API request
        const deviceIdResult = HelperFunctions.getDeviceId();
        if (!deviceIdResult.success || !deviceIdResult.data)
            return deviceIdResult.convertTo<User>();

        const accessPoint = "user/login-with-code";
        const url = HelperFunctions.createUrlRequest(accessPoint);
        const options: FetchOptions = {
            method: "POST",
            headers: {
                "Device-Id": deviceIdResult.data,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
        const result = await HelperFunctions.executeFetchWithTimeout(url, options);
        if (result.code) result.component = Components.USER_SERVICE;
        if (!result.success || !result.data) return result.convertTo<User>();

        // Process response
        const responseData = result.data;
        const user = new User(
            responseData.id,
            responseData.name,
            responseData.email,
            responseData.documentType,
            responseData.documentNumber
        );
        HelperFunctions.updateSessionToken(responseData.sessionToken);
        HelperFunctions.saveLocalUserSession(user);
        return Result.ok(user, null, Components.USER_SERVICE, result.code);
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<User>(
                `Error during login, error: ${error.message}`,
                500,
                Components.USER_SERVICE,
                "USER_LOGIN_ERROR"
            ).log();
    }
}