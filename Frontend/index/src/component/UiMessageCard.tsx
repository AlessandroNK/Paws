import type {UiMessage} from "../types/MessageTypes.ts";
import "./UiMessageCard.css";

interface Props {
    message: UiMessage
}

function UiMessageCard(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className={"ui-message " + "ui-message-" + props.message.type}>
            {props.message.message}
        </div>
    );
}

export default UiMessageCard;