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