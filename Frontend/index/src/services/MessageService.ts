import {type Message, MessageType} from "../types/MessageTypes.ts";

// ---------------------------------------------------------------------------------------------------------------------
/**
 * Logs a message to the console with appropriate formatting based on the message type.
 * @param message The message object containing the type, component, and message text to be logged
 */
export function log(message: Message) {
    switch (message.type) {
        case MessageType.INFO:
            console.info(`[INFO] ${message.component ? `[${message.component}] ` : ''}${message.message}`);
            break;
        case MessageType.WARNING:
            console.warn(`[WARNING] ${message.component ? `[${message.component}] ` : ''}${message.message}`);
            break;
        case MessageType.ERROR:
            console.error(`[ERROR] ${message.component ? `[${message.component}] ` : ''}${message.message}`);
            break;
        case MessageType.SUCCESS:
            console.log(`[SUCCESS] ${message.component ? `[${message.component}] ` : ''}${message.message}`);
            break;
        default:
            console.log(`[UNKNOWN] ${message.component ? `[${message.component}] ` : ''}${message.message}`);
    }
}