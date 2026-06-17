import type {Appointment} from "../types/SystemTypes.ts";
import {Components, Day, FetchOptions, Result} from "../types/CommonTypes.ts";
import * as HelperFunctions from "../resources/HelperFunctions.ts";

//                                                                                                             FUNCTIONS
// ---------------------------------------------------------------------------------------------------------------------
export async function getAvailableAppointmentsApi(request: Day): Promise<Result<Appointment[]>> {
    try {
        // API request
        const deviceIdResult = HelperFunctions.GetDeviceId();
        if (!deviceIdResult.success || !deviceIdResult.data)
            return deviceIdResult.convertTo<Appointment[]>();

        const accessPoint = "appointment/get-available-appointments";
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
        if (!result.success || !result.data) return result.convertTo<Appointment[]>();

        // Process response
        return createAppointmentsFromApiResponse(result.data);
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<Appointment[]>(
                `Error fetching appointments, error: ${error.message}`,
                500,
                Components.APPOINTMENT_SERVICE,
                "APPOINTMENTS_FETCH_ERROR"
            ).log();
    }
}

// ---------------------------------------------------------------------------------------------------------------------
function createAppointmentsFromApiResponse(apiResponse: object): Result<Appointment[]> {
    // Validate API response format
    if (!Array.isArray(apiResponse)) {
        return Result.fail<Appointment[]>(
            "Invalid API response format: expected an array of appointments",
            500,
            Components.API_RESPONSE_PROCESSING,
            "INVALID_APPOINTMENTS_RESPONSE"
        ).log();
    }

    // Process each appointment item in the response
    const appointments: Appointment[] = [];
    for (const item of apiResponse) {
        const appointmentResult = createAppointmentFromApiResponse(item);
        if (!appointmentResult.success || !appointmentResult.data) {
            return Result.fail<Appointment[]>(
                "Error processing API response: invalid appointment data",
                500,
                Components.API_RESPONSE_PROCESSING,
                "INVALID_APPOINTMENT_DATA"
            ).log();
        }
        appointments.push(appointmentResult.data);
    }

    // Return it
    return Result.ok(appointments);
}

// ---------------------------------------------------------------------------------------------------------------------
function createAppointmentFromApiResponse(apiResponse: any): Result<Appointment> {
    return Result.ok(new Appointment(
        apiResponse.id,
        apiResponse.petId
    ));
}