import {Message, MessageType, UiMessage} from "./MessageTypes.ts";
import * as MessageService from "../services/MessageService.ts";
import * as LanguageService from "../services/LanguageService.ts";

// ---------------------------------------------------------------------------------------------------------------------
export const Components = {
    NONE: 'none',
    SIDEBAR: 'sidebar',
    CALENDAR: 'calendar',
    HELPER_FUNCTIONS: 'helper-functions',
    HTTP_SERVICE: 'http-service',
    APPOINTMENT_SERVICE: 'appointment-service',
    API_RESPONSE_PROCESSING: 'api-response-processing',
} as const;

export type Components = typeof Components[keyof typeof Components];

// ---------------------------------------------------------------------------------------------------------------------
export class ApiError {
    title: string;
    messages: UiMessage[];

    constructor(title: string) {
        this.title = title;
        this.messages = []
    }

    // ------------------------------------------------------------------------
    addMessage(message: string): ApiError {
        const uiMessage = new UiMessage();
        uiMessage.type = MessageType.ERROR;
        uiMessage.message = message;
        this.messages.push(uiMessage);
        return this;
    }

    // ------------------------------------------------------------------------
    addUiMessage(uiMessage: UiMessage): ApiError {
        this.messages.push(uiMessage);
        return this;
    }
}

// ---------------------------------------------------------------------------------------------------------------------
/**
 * The Result class is a generic class that represents the outcome of an operation, encapsulating information about the
 * success or failure of the operation, any relevant data, and messages for UI display. It provides static methods for
 * creating successful and failed results, as well as a method for converting the result to a different generic type and
 * a method for logging the result's messages to the console using the MessageService.
 */
export class Result<T> {
    success: boolean;
    component: Components;
    code: string | null;
    status: number | null;
    title: string | null;
    data: T | null;
    errors: ApiError[];

    // ------------------------------------------------------------------------
    constructor(
        success: boolean,
        component: Components,
        code: string | null = null,
        status: number | null = null,
        title: string | null = null,
        data: T | null = null,
    ) {
        this.success = success;
        this.component = component;
        this.code = code;
        this.status = status;
        this.title = title;
        this.data = data;
        this.errors = [];
    }

    // ------------------------------------------------------------------------
    /**
     * Creates a successful Result object with the provided data, title, component, code, and status. The success
     * property is set to true, and the errors array is initialized as empty. The title, code, and status parameters are
     * optional and can be null if not provided.
     * @param data The data to be included in the Result object, of generic type T
     * @param title An optional title for the Result, which can be used for UI display or logging purposes
     * @param component The component associated with the Result, which can be used for categorizing or identifying the
     * source of the result
     * @param code An optional code that can be used to further specify the type of result or error, which can be useful
     * for handling specific cases in the application
     * @param status An optional HTTP status code that can be associated with the Result, which can be useful for API
     * responses or error handling
     * @return A new Result object with the success property set to true, containing the provided data and optional title,
     * component, code, and status, and an empty errors array
     */
    static ok<T>(
        data: T,
        title: string | null = null,
        component: Components = Components.NONE,
        code: string | null = null,
        status: number = 200,
    ): Result<T> {
        return new Result<T>(
            true,
            component,
            code,
            status,
            title,
            data
        );
    }

    // ------------------------------------------------------------------------
    /**
     * Creates a failed Result object with the provided title, status, component, and code. The success property is set to
     * false, and the data property is set to null. The errors array is initialized as empty. The title, component, code,
     * and status parameters are required to provide context about the failure, which can be useful for UI display, logging,
     * and error handling in the application.
     * @param title A title for the failed Result, which can be used for UI display or logging purposes to indicate the
     * nature of the failure
     * @param status An HTTP status code associated with the failure, which can be useful for API responses or error
     * handling to indicate the type of error that occurred
     * @param component The component associated with the failed Result, which can be used for categorizing or identifying
     * the source of the failure, and can help in debugging and error tracking within the application
     * @param code An optional code that can be used to further specify the type of failure, which can be useful for handling
     * specific cases in the application, and can provide additional context about the error that occurred
     * @return A new Result object with the success property set to false, containing the provided title, status, component,
     * and code, with the data property set to null and an empty errors array
     */
    static fail<T>(
        title: string,
        status: number,
        component: Components,
        code: string | null = null
    ): Result<T> {
        return new Result<T>(
            false,
            component,
            code,
            status,
            title,
            null
        );
    }

    // ------------------------------------------------------------------------
    /**
     * Converts the current Result object to a new Result object with a different generic type U, while preserving the
     * success, status, message, and uiMessage properties. The data property of the new Result object is set to null, as
     * the conversion does not carry over the original data.
     * @return A new Result object with the same success, status, message, and uiMessage properties as the original, but
     * with a generic type U and null data
     */
    convertTo<U>(): Result<U> {
        return new Result<U>(
            this.success,
            this.component,
            this.code,
            this.status,
            this.title,
            null
        ).addErrors(this.errors);
    }

    // ------------------------------------------------------------------------
    addError(error: ApiError): Result<T> {
        this.errors.push(error);
        return this;
    }

    // ------------------------------------------------------------------------
    addErrors(errors: ApiError[]): Result<T> {
        this.errors.push(...errors);
        return this;
    }

    // ------------------------------------------------------------------------
    /**
     * Logs the message contained in the Result object to the console using the MessageService, with the appropriate type
     * (SUCCESS or ERROR) based on the success property of the Result. The method then returns the Result object itself
     * for potential chaining of further operations.
     */
    log(): Result<T> {
        if (this.title) {
            const consoleMessage = new Message();
            consoleMessage.type = this.success ? MessageType.SUCCESS : MessageType.ERROR;
            consoleMessage.message = this.title;
            MessageService.log(consoleMessage);
        }

        // Errors
        if (this.errors.length > 0) {
            this.errors.forEach((error) => {
                for (const errorElement of error.messages) {
                    const errorMessage = new Message();
                    errorMessage.type = errorElement.type;
                    errorMessage.message = `[${error.title}] ${errorElement.message}`;
                    MessageService.log(errorMessage);
                }
            });
        }

        return this;
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export class FetchOptions {
    method: string;
    headers: Record<string, string>;
    body?: string;
    signal?: AbortSignal;

    constructor(
        method: string,
        headers: Record<string, string>,
        body?: never,
        signal?: AbortSignal
    ) {
        this.method = method;
        this.headers = headers;
        this.body = body;
        this.signal = signal;
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export class Day {
    public Year: number;
    public Month: number;
    public Day: number;

    constructor(year: number, month: number, day: number) {
        this.Year = year;
        this.Month = month;
        this.Day = day;
    }

    // ------------------------------------------------------------------------
    public static fromDate(date: Date): Day {
        return new Day(date.getFullYear(), date.getMonth() + 1, date.getDate());
    }

    // ------------------------------------------------------------------------
    public toDate(): Date {
        return new Date(this.Year, this.Month - 1, this.Day);
    }

    // ------------------------------------------------------------------------
    public toString(): string {
        const monthString = this.Month.toString().padStart(2, "0");
        const dayString = this.Day.toString().padStart(2, "0");
        return `${this.Year}-${monthString}-${dayString}`;
    }

    // ------------------------------------------------------------------------
    public getDayOfWeek(): number {
        const date = this.toDate();
        return date.getDay();
    }

    // ------------------------------------------------------------------------
    public getDayOfWeekName(): string {
        return LanguageService.getDayOfWeek(this.getDayOfWeek());
    }

    // ------------------------------------------------------------------------
    public getMonth(): number {
        return this.Month;
    }

    // ------------------------------------------------------------------------
    public getMonthName(): string{
        return LanguageService.getMonth(this.Month);
    }

    // ------------------------------------------------------------------------
    public getYear(): number {
        return this.Year;
    }

    // ------------------------------------------------------------------------
    public isSameDay(other: Day): boolean {
        return this.Year === other.Year && this.Month === other.Month && this.Day === other.Day;
    }

    // ------------------------------------------------------------------------
    public async printFormated(): Promise<string> {
        const monthString = LanguageService.getMonth(this.Month);
        const ofResult = await LanguageService.getTranslationAsync(Components.CALENDAR, "OF");
        if (!ofResult.success) return `${LanguageService.getDayOfWeek(this.getDayOfWeek())} ${this.Day} ${monthString} ${this.Year}`;
        const of = ofResult.data ?? "of";
        return `${LanguageService.getDayOfWeek(this.getDayOfWeek())} ${this.Day} ${of} ${monthString} ${of} ${this.Year}`;
    }
}