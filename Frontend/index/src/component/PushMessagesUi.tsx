import "./PushMessagesUi.css";
import type {UiMessage} from "../types/MessageTypes.ts";
import UiMessageCard from "./UiMessageCard.tsx";

interface Props {
    messages: UiMessage[]
}

function PushMessagesUi(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------


    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <section className={"push-messages-interface"}>
            {props.messages.map((message) => (
                <UiMessageCard key={message.id} message={message}/>
            ))}
        </section>
    );
}

export default PushMessagesUi;