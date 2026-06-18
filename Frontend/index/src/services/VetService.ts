import type {VetResponse} from "../types/ResponseTypes.ts";
import {Components, Result} from "../types/CommonTypes.ts";
import {Vet} from "../types/SystemTypes.ts";

//                                                                                                             FUNCTIONS
// ---------------------------------------------------------------------------------------------------------------------
export function createVetFromApiResponse(apiResponse: VetResponse): Result<Vet> {
    // Validate API response format
    if (typeof apiResponse !== "object" || apiResponse === null) {
        return Result.fail<Vet>(
            "Invalid API response format: expected an object",
            500,
            Components.API_RESPONSE_PROCESSING,
            "INVALID_VET_ITEM_RESPONSE"
        ).log();
    }

    // Create appointment
    return Result.ok(new Vet(
        apiResponse.id,
        "",
        0,
        "",
        apiResponse.name,
        "",
        apiResponse.profilePicture,
        apiResponse.funFact,
        new Date(apiResponse.vetSince)
    ));
}