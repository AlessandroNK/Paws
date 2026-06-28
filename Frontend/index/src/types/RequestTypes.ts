export class StartLoginRequest {
    public email: string;

    constructor(email: string) {
        this.email = email;
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export class LoginRequest {
    public email: string;
    public code: string;

    constructor(email: string, code: string) {
        this.email = email;
        this.code = code;
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export class ReserveAppointmentRequest {
    public appointmentId: number;
    public userId: number;
    public petId: number;

    constructor(appointmentId: number, userId: number, petId: number) {
        this.appointmentId = appointmentId;
        this.userId = userId;
        this.petId = petId;
    }
}