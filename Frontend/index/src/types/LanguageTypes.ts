// ---------------------------------------------------------------------------------------------------------------------
export const Languages = {
    ENGLISH: 'en_US',
    SPANISH: 'es_LA',
} as const;

export type Languages = typeof Languages[keyof typeof Languages];