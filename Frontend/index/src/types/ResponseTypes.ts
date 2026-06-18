export class AppointmentResponse {
    "id": number;
    "vetId": number;
    "vet": VetResponse;
    "userPetId": number | null;
    "startTime": string;
    "endTime": string;
    "createdAt": null;
    "updatedAt": null;
}

// ---------------------------------------------------------------------------------------------------------------------
export class VetResponse {
    "id": number;
    "name": string;
    "profilePicture": string;
    "funFact": string;
    "vetSince": string;
}