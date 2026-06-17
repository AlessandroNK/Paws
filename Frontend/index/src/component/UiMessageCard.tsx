import {Message, MessageType, type UiMessage} from "../types/MessageTypes.ts";
import "./UiMessageCard.css";
import {useEffect, useState} from "react";
import * as LanguageService from "../services/LanguageService.ts";
import * as MessageService from "../services/MessageService.ts";

interface Props {
    message: UiMessage
}

function UiMessageCard(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const [message, setMessage] = useState("");
    const [showCard, setShowCard] = useState(true)

    // -----------------------------------------------------------------------------------------------------------------
    useEffect(() => {
        // Load the translation of the message
        async function loadTranslation() {
            if (props.message.message) {
                // If there is a messag,e the use it as it
                setMessage(props.message.message);
            } else if (props.message.code) {
                // If not, then use the code to translate the message
                const result = await LanguageService.getTranslation(
                    props.message.component,
                    props.message.code
                );
                setMessage(result.data ?? `${props.message.code}::${props.message.component}`);
            } else {
                // If there is no message and no code
                // Log the error
                const message = new Message();
                message.message = "Message not found";
                message.type = MessageType.ERROR;
                MessageService.log(message);

                // Then hide the card
                setShowCard(false);
            }
        }

        loadTranslation();
    }, []);

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    if (!showCard) return null;

    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className={"ui-message " + "ui-message-" + props.message.type}>
            {message}
        </div>
    );
}

export default UiMessageCard;