import "./PawsChat.css"
import type {User} from "../types/SystemTypes.ts";
import {useState} from "react";
import * as HelperFunctions from "../resources/HelperFunctions.ts";
import Logo from "./Logo.tsx";

interface Props {
    user: User | null;
}

function pawsChat(props: Props) {
    const [sendingMessage, setSendingMessage] = useState<boolean>(false);
    const [pawsMessage, setPawsMessage] = useState<string>(`Hola, ${props.user?.name}! Como puedo ayudarte?`);
    const [userMessage, setUserMessage] = useState<string>("");

    // -----------------------------------------------------------------------------------------------------------------
    async function handleSendMessage() {
        setPawsMessage("");

        if (!props.user) {
            console.error("User not logged in. Cannot send message.");
            return;
        }

        setSendingMessage(true);
        const accessPoint = "chat";
        const url = HelperFunctions.createUrlRequest(accessPoint);

        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                UserName: props.user?.name || "Usuario",
                message: userMessage
            })
        });

        const reader = response.body.getReader();
        const decoder = new TextDecoder();

        let fullMessage = "";
        while (true) {
            const {done, value} = await reader.read();
            if (done) break;
            fullMessage += decoder.decode(value);
            setPawsMessage(fullMessage);
        }
        setSendingMessage(false);
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className="paws-chat">
            <div className="paws-chat-header">
                <Logo className={"h-8 fill-main"}/>
            </div>
            <div className="paws-chat-body">
                {sendingMessage && !pawsMessage && <div className="paws-writing"></div>}
                {pawsMessage && (
                    <div className="paws-chat-response">
                        <p>{pawsMessage}</p>
                    </div>
                )}
            </div>
            <div className="paws-chat-footer">
                <input type="text"
                       value={userMessage}
                       onChange={(e) => setUserMessage(e.target.value)}
                       placeholder="Escribe tu mensaje..."
                       disabled={!props.user}
                />
                <button disabled={!props.user || sendingMessage}
                        className={!props.user || sendingMessage ? "disabled" : ""}
                        onClick={handleSendMessage}>
                    Send
                </button>
            </div>
        </div>
    );
}

export default pawsChat;