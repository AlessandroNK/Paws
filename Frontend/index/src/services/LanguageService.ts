import {Languages} from "../types/LanguageTypes.ts";
import {Components, Result} from "../types/CommonTypes.ts";

//                                                                                                             CONSTANTS
// ---------------------------------------------------------------------------------------------------------------------
const englishDaysOfWeek = [
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday"
];

const englishMonths = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December"
];

const spanishDaysOfWeek = [
    "Domingo",
    "Lunes",
    "Martes",
    "Miércoles",
    "Jueves",
    "Viernes",
    "Sábado"
];

const spanishMonths = [
    "Enero",
    "Febrero",
    "Marzo",
    "Abril",
    "Mayo",
    "Junio",
    "Julio",
    "Agosto",
    "Septiembre",
    "Octubre",
    "Noviembre",
    "Diciembre"
];


//                                                                                                             VARIABLES
// ---------------------------------------------------------------------------------------------------------------------
let currentLanguage: Languages = Languages.SPANISH;

//                                                                                                             FUNCTIONS
// ---------------------------------------------------------------------------------------------------------------------
export function setLanguage(language: Languages): void {
    currentLanguage = language;
}

// ---------------------------------------------------------------------------------------------------------------------
/**
 * Fetches the translation for a given component and message code from the corresponding language file.
 * @param component The component for which the translation is needed, used to determine which language file to fetch.
 * @param code The specific message code within the language file that corresponds to the desired translation.
 * @returns A Result object containing the translated message as a string if successful, or an error message if the
 * operation fails.
 * @description This function constructs the path to the appropriate language file based on the current language and
 * component, fetches the file, and retrieves the translation for the specified message code. It includes error handling
 * for cases where the language file is not found or the message code does not exist within the file.
 * @example
 * // Assuming currentLanguage is set to Languages.SPANISH and there is a file at ./lang/spanish/calendar.json with a
 * message code "APPOINTMENTS_TITLE":
 * getTranslation(Components.CALENDAR, "APPOINTMENTS_TITLE"); // returns Result.ok("Título de las citas")
 */
export async function getTranslationAsync(component: Components, code: string): Promise<Result<string>> {
    try {
        const languageFile = await fetch(`./lang/${currentLanguage}/${component}.json`);
        if (!languageFile.ok) {
            return Result
                .fail<string>(
                    `Error decoding ${component} message. Language file not found.`,
                    404,
                    Components.HELPER_FUNCTIONS,
                    "LANGUAGE_FILE_NOT_FOUND"
                ).log();
        }

        const languageFileJson = await languageFile.json();
        const message = languageFileJson[code];

        // Validations
        if (!message) {
            return Result
                .fail<string>(
                    `Error decoding ${component}::${code} message. Message code not found in language file.`,
                    404,
                    Components.HELPER_FUNCTIONS,
                    "MESSAGE_CODE_NOT_FOUND"
                ).log();
        }

        // If the message is a string, return it directly
        if (!Array.isArray(message))
            return Result.ok(message);

        // Pick one of the messages
        const finalMessage = message[Math.floor(Math.random() * message.length)];
        return Result.ok(finalMessage);
    } catch (err) {
        const error = err as Error;
        return Result
            .fail<string>(
                `Error fetching translation for ${component}::${code}, error: ${error.message}`,
                500,
                Components.HELPER_FUNCTIONS,
                "TRANSLATION_FETCH_ERROR"
            ).log();
    }
}

// ---------------------------------------------------------------------------------------------------------------------
/**
 * Returns the name of the day of the week corresponding to the given index (0 for Sunday, 1 for Monday, etc.) based on
 * the current language setting. It uses predefined arrays of day names for English and Spanish to retrieve the correct
 * name.
 * @param dayIndex An integer representing the index of the day of the week (0 for Sunday, 1 for Monday, etc.).
 * @returns A string representing the name of the day of the week in the current language.
 * @description This function checks the current language setting and returns the appropriate day name from the
 * corresponding array. It assumes that the dayIndex provided is valid (between 0 and 6) and does not include error
 * handling for out-of-range indices.
 * @example
 * // If currentLanguage is set to Languages.ENGLISH:
 * getDayOfWeek(0); // returns "Sunday"
 * getDayOfWeek(1); // returns "Monday"
 * // If currentLanguage is set to Languages.SPANISH:
 * getDayOfWeek(0); // returns "Domingo"
 * getDayOfWeek(1); // returns "Lunes"
 */
export function getDayOfWeek(dayIndex: number): string {
    switch (currentLanguage) {
        case Languages.ENGLISH:
            return englishDaysOfWeek[dayIndex];
        case Languages.SPANISH:
            return spanishDaysOfWeek[dayIndex];

    }
}

// ---------------------------------------------------------------------------------------------------------------------
/**
 * Returns the name of the month corresponding to the given index (0 for January, 1 for February, etc.) based on the
 * current language setting. It uses predefined arrays of month names for English and Spanish to retrieve the correct
 * name.
 * @param monthIndex An integer representing the index of the month (0 for January, 1 for February, etc.).
 * @returns A string representing the name of the month in the current language.
 * @description This function checks the current language setting and returns the appropriate month name from the
 * corresponding array. It assumes that the monthIndex provided is valid (between 0 and 11) and does not include error
 * handling for out-of-range indices.
 * @example
 * // If currentLanguage is set to Languages.ENGLISH:
 * getMonth(0); // returns "January"
 * getMonth(1); // returns "February"
 * // If currentLanguage is set to Languages.SPANISH:
 * getMonth(0); // returns "Enero"
 * getMonth(1); // returns "Febrero"
 */
export function getMonth(monthIndex: number): string {
    switch (currentLanguage) {
        case Languages.ENGLISH:
            return englishMonths[monthIndex];
        case Languages.SPANISH:
            return spanishMonths[monthIndex];
    }
}