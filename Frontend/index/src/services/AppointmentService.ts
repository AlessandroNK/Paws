import {Appointment} from "../types/SystemTypes.ts";
import {Components, Day, FetchOptions, Result} from "../types/CommonTypes.ts";
import * as HelperFunctions from "../resources/HelperFunctions.ts";
import type {AppointmentResponse} from "../types/ResponseTypes.ts";
import * as VetService from "./VetService.ts";
import type {ReserveAppointmentRequest} from "../types/RequestTypes.ts";

//                                                                                                             FUNCTIONS
// ---------------------------------------------------------------------------------------------------------------------
export async function getAvailableAppointmentsApi(request: Day): Promise<Result<Appointment[]>> {
    try {
        // API request
        const deviceIdResult = HelperFunctions.getDeviceId();
        if (!deviceIdResult.success || !deviceIdResult.data)
            return deviceIdResult.convertTo<Appointment[]>();

        const accessPoint = "appointment/get-available-appointments";
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
        if (result.code) result.component = Components.APPOINTMENT_SERVICE;
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
export async function reserveAppointmentAsync(request: ReserveAppointmentRequest): Promise<Result<Appointment>> {
    try {
        // API request
        const deviceIdResult = HelperFunctions.getDeviceId();
        if (!deviceIdResult.success || !deviceIdResult.data)
            return deviceIdResult.convertTo<Appointment>();

        const accessPoint = "appointment/reserve-appointment";
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
        if (result.code) result.component = Components.APPOINTMENT_SERVICE;
        if (!result.success || !result.data) return result.convertTo<Appointment>();

        // Process response
        return createAppointmentFromApiResponse(result.data);
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<Appointment>(
                `Error reserving appointment, error: ${error.message}`,
                500,
                Components.APPOINTMENT_SERVICE,
                "APPOINTMENT_RESERVATION_ERROR"
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
function createAppointmentFromApiResponse(apiResponse: AppointmentResponse): Result<Appointment> {
    // Validate API response format
    if (typeof apiResponse !== "object" || apiResponse === null) {
        return Result.fail<Appointment>(
            "Invalid API response format: expected an object",
            500,
            Components.API_RESPONSE_PROCESSING,
            "INVALID_APPOINTMENT_ITEM_RESPONSE"
        ).log();
    }

    // Create dates
    const startTime: Date = new Date(apiResponse.startTime);
    const endTime: Date = new Date(apiResponse.endTime);

    // Create appointment
    const appointment = new Appointment(
        apiResponse.id,
        apiResponse.vetId,
        startTime,
        endTime,
        null,
        null
    );

    const vetResult = VetService.createVetFromApiResponse(apiResponse.vet);
    if (!vetResult.success || !vetResult.data) {
        return Result.fail<Appointment>(
            "Error processing API response: invalid vet data",
            500,
            Components.API_RESPONSE_PROCESSING,
            "INVALID_VET_DATA"
        ).log();
    }

    appointment.vet = vetResult.data;
    return Result.ok(appointment);
}