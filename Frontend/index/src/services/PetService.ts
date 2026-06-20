//                                                                                                             FUNCTIONS
// ---------------------------------------------------------------------------------------------------------------------
import {Components, FetchOptions, Result} from "../types/CommonTypes.ts";
import {Pet, User} from "../types/SystemTypes.ts";
import * as HelperFunctions from "../resources/HelperFunctions.ts";

export async function getPetsByOwnerApi(sessionToken: string, ownerId: number): Promise<Result<Pet[]>> {
    try {
        // API request
        const deviceIdResult = HelperFunctions.getDeviceId();
        if (!deviceIdResult.success || !deviceIdResult.data)
            return deviceIdResult.convertTo<Pet[]>();

        if (!sessionToken) {
            return Result.fail<Pet[]>(
                "No session token found for the user",
                401,
                Components.USER_SERVICE,
                "USER_SESSION_TOKEN_MISSING"
            ).log();
        }

        if (!ownerId || isNaN(Number(ownerId)) || Number(ownerId) <= 0) {
            return Result.fail<Pet[]>(
                "Invalid owner ID provided",
                400,
                Components.PET_SERVICE,
                "INVALID_OWNER_ID"
            ).log();
        }

        const accessPoint = "pet/get-pets-by-owner";
        const url = HelperFunctions.createUrlRequest(accessPoint);
        const options: FetchOptions = {
            method: "POST",
            headers: {
                "Device-Id": deviceIdResult.data,
                "Session-Token": sessionToken,
                "Content-Type": "application/json"
            },
                body: JSON.stringify({ ownerId })
        }
        const result = await HelperFunctions.executeFetchWithTimeout(url, options);
        if (result.code) result.component = Components.PET_SERVICE;
        if (!result.success || !result.data) return result.convertTo<Pet[]>();
        if (result.data.pets.length === 0) {
            return Result.fail<Pet[]>(
                "No pets found for the specified owner",
                404,
                Components.PET_SERVICE,
                "PETS_NOT_FOUND"
            ).log();
        }

        // Process response
        const pets: Pet[] = result.data.pets.map((petData: object) => new Pet(
            petData.id,
            petData.name,
            petData.species,
            petData.breed,
            petData.age,
            petData.ownerId
        ));
        return Result.ok(pets);
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<Pet[]>(
                `Error fetching pets for owner, error: ${error.message}`,
                500,
                Components.PET_SERVICE,
                "PETS_FETCH_ERROR"
            ).log();
    }
}