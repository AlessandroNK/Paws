// ---------------------------------------------------------------------------------------------------------------------
export class Vet {
    public id: number
    public email: string
    public documentType: number
    public documentNumber: string
    public name: string
    public professionalLicenseNumber: string
    public profilePicture: string
    public funFact: string
    public vetSince: Date

    constructor(
        id: number,
        email: string,
        documentType: number,
        documentNumber: string,
        name: string,
        professionalLicenseNumber: string,
        profilePicture: string,
        funFact: string,
        vetSince: Date,
    ) {
        this.id = id;
        this.email = email;
        this.documentType = documentType;
        this.documentNumber = documentNumber;
        this.name = name;
        this.professionalLicenseNumber = professionalLicenseNumber;
        this.profilePicture = profilePicture;
        this.funFact = funFact;
        this.vetSince = vetSince;
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export class Appointment {
    public id: number
    public vetId: number
    public vet?: Vet
    public userPetId?: number | null
    public startTime: Date
    public endTime: Date
    public createdAt: Date
    public updatedAt: Date

    constructor(
        id: number,
        vetId: number,
        startTime: Date,
        endTime: Date,
        createdAt: Date,
        updatedAt: Date,
        vet?: Vet,
        userPetId?: number | null,
    ) {
        this.id = id;
        this.vetId = vetId;
        this.startTime = startTime;
        this.endTime = endTime;
        this.createdAt = createdAt;
        this.updatedAt = updatedAt;
        this.vet = vet;
        this.userPetId = userPetId ?? null;
    }
}