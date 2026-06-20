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

    /**
     * Creates an instance of the Vet class.
     * @param id The unique identifier for the veterinarian.
     * @param email The email address of the veterinarian.
     * @param documentType The type of document used for identification (e.g., passport, driver's license).
     * @param documentNumber The number associated with the identification document.
     * @param name The full name of the veterinarian.
     * @param professionalLicenseNumber The professional license number of the veterinarian, which is typically issued by
     * a regulatory body to certify that the veterinarian is qualified to practice.
     * @param profilePicture A URL or path to the profile picture of the veterinarian.
     * @param funFact A fun fact about the veterinarian, which can be used to add a personal touch to their profile.
     * @param vetSince The date when the veterinarian started practicing, which can be used to calculate their experience in the field.
     */
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
    public createdAt: Date | null
    public updatedAt: Date | null

    /**
     * Creates an instance of the Appointment class.
     * @param id The unique identifier for the appointment.
     * @param vetId The unique identifier for the veterinarian associated with the appointment.
     * @param startTime The starting time of the appointment.
     * @param endTime The ending time of the appointment.
     * @param createdAt The date and time when the appointment was created.
     * @param updatedAt The date and time when the appointment was last updated.
     * @param vet (Optional) The veterinarian associated with the appointment. This can be provided if the vet details
     * are available at the time of creating the appointment instance.
     * @param userPetId (Optional) The unique identifier for the user's pet associated with the appointment. This can be
     * null if no pet is associated or if the information is not available at the time of creating the appointment instance.
     */
    constructor(
        id: number,
        vetId: number,
        startTime: Date,
        endTime: Date,
        createdAt: Date | null,
        updatedAt: Date | null,
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

// ---------------------------------------------------------------------------------------------------------------------
export class TimePeriod {
    public id: string
    public startTime: Date
    public endTime: Date
    public appointments: Appointment[] = []

    constructor(id: string, startTime: Date, endTime: Date) {
        this.id = id;
        this.startTime = startTime;
        this.endTime = endTime;
        this.appointments = [];
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export class User {
    public id: number
    public name: string
    public email: string
    public documentType: number
    public documentNumber: string | null
    public sessionToken: string | null
    public pets: Pet[]

    constructor(
        id: number,
        name: string,
        email: string,
        documentType: number = 1,
        documentNumber: string | null = null,
        sessionToken: string | null = null,
    ) {
        this.id = id;
        this.name = name;
        this.email = email;
        this.documentType = documentType;
        this.documentNumber = documentNumber;
        this.sessionToken = sessionToken;
        this.pets = [];
    }

    // ------------------------------------------------------------------------
    public getInitials(): string {
        const nameParts = this.name.split(" ");
        let initials = "";
        for (const part of nameParts) {
            if (part.length > 0) {
                initials += part[0].toUpperCase();
            }
        }
        return initials;
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export const PetSpecies = {
    Other: 1,
    Dog: 2,
    Cat: 3,
    Bunny: 4,
    Hamster: 5,
    Turtle: 6,
    Cow: 7,
    Horse: 8,
    Bird: 9,
    Fish: 10,
    Reptile: 11,
    Rodent: 12
} as const;

export type PetSpecies = typeof PetSpecies[keyof typeof PetSpecies];

// ---------------------------------------------------------------------------------------------------------------------
export class Pet {
    public id: number;
    public name: string;
    public species: PetSpecies;
    public breed: string | null;
    public createdAt: Date | null;
    public updatedAt: Date | null;

    // public ownershipInvitations: OwnershipInvitation[];

    constructor(
        id: number,
        name: string,
        species: PetSpecies = PetSpecies.Other,
        breed: string| null = null,
        createdAt: Date | null = null,
        updatedAt: Date | null = null,
    ) {
        this.id = id;
        this.name = name;
        this.species = species;
        this.breed = breed;
        this.createdAt = createdAt;
        this.updatedAt = updatedAt;
    }
}