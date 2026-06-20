import "./LoginCard.css";
import {useState} from "react";
import type {Result} from "../types/CommonTypes.ts";
import * as LanguageService from "../services/LanguageService.ts";
import {MessageType, UiMessage} from "../types/MessageTypes.ts";

interface Props {
    className?: string,
    onClose: () => void,
    onStartLogin: (email: string) => Promise<Result<void>>,
    onLoginWithCode: (email: string, code: string) => Promise<Result<void>>,
}

function LoginCard(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const [email, setEmail] = useState('');
    const [loginMessage, setLoginMessage] = useState('');
    const [loginClass, setLoginClass] = useState('');
    const [isFetchingApi, setIsFetchingApi] = useState(false);
    const [codeSent, setCodeSent] = useState(false);
    const [code, setCode] = useState('');


    // Functions
    // -----------------------------------------------------------------------------------------------------------------
    async function showUiMessage(uiMessage: UiMessage) {
        let className: string;
        switch (uiMessage.type) {
            case MessageType.WARNING:
                className = 'login-form-warning-message';
                break;
            case MessageType.SUCCESS:
                className = 'login-form-success-message';
                break;
            case MessageType.ERROR:
                className = 'login-form-error-message';
                break;
            default:
                className = 'login-form-info-message';
        }
        setLoginClass(className);

        if (uiMessage.message) {
            setLoginMessage(uiMessage.message);
            return;
        }

        const result = await LanguageService.getTranslationAsync(uiMessage.component, uiMessage.code ?? "UNKNOWN_ERROR");
        setLoginMessage(result.data ?? "Error desconocido");
    }

    // -----------------------------------------------------------------------------------------------------------------
    async function handleStartLoginProcess() {
        if (isFetchingApi) {
            const uiMessage = new UiMessage();
            uiMessage.message = "Ya se está procesando una solicitud, por favor espera.";
            uiMessage.type = MessageType.WARNING;
            showUiMessage(uiMessage);
            return;
        }

        // UI
        setIsFetchingApi(true);
        setLoginMessage('');
        setLoginClass('login-form-info-message');

        // Yeah I know I can write more clean code but
        // I got not time left
        if (!email) {
            const uiMessage = new UiMessage();
            uiMessage.message = "Por favor ingresa tu correo electrónico para iniciar sesión.";
            uiMessage.type = MessageType.WARNING;
            await showUiMessage(uiMessage);
            setIsFetchingApi(false);
            return;
        }

        if (!codeSent) await handleStartLogin();
        else await handleLoginWithCode();
    }

    // -----------------------------------------------------------------------------------------------------------------
    async function handleStartLogin() {
        // API request
        const loginResult = await props.onStartLogin(email);
        setIsFetchingApi(false);

        if (!loginResult.success) {
            const uiMessage = new UiMessage();
            uiMessage.component = loginResult.component;
            uiMessage.code = loginResult.code ?? "UNKNOWN_ERROR";
            uiMessage.type = MessageType.ERROR;
            await showUiMessage(uiMessage);
            return;
        }

        setIsFetchingApi(false);
        if (loginResult.code === "LOGIN_CODE_SENT") {
            setCodeSent(true);
            const uiMessage = new UiMessage();
            uiMessage.message = "Se ha enviado un código de inicio de sesión a tu correo electrónico.";
            uiMessage.type = MessageType.SUCCESS;
            showUiMessage(uiMessage);
            return;
        }

        const uiMessage = new UiMessage();
        uiMessage.message = "No podemos continuar con el proceso de inicio de sesión, por favor intenta nuevamente más tarde.";
        uiMessage.type = MessageType.ERROR;
        showUiMessage(uiMessage);
    }

    // -----------------------------------------------------------------------------------------------------------------
    async function handleLoginWithCode() {
        // API request
        const loginResult = await props.onLoginWithCode(email, code);
        setIsFetchingApi(false);

        if (loginResult.code === 'LOGIN_CODE_EXPIRED') {
            const uiMessage = new UiMessage();
            uiMessage.message = "El código de inicio de sesión ha expirado. Por favor, solicita uno nuevo.";
            uiMessage.type = MessageType.WARNING;
            showUiMessage(uiMessage);
            setCode('');
            setCodeSent(false);
            return;
        }

        if (loginResult.code === 'LOGIN_SUCCESSFUL') {
            props.onClose();
            return;
        }

        if (!loginResult.success) {
            const uiMessage = new UiMessage();
            uiMessage.component = loginResult.component;
            uiMessage.code = loginResult.code ?? "UNKNOWN_ERROR";
            uiMessage.type = MessageType.ERROR;
            await showUiMessage(uiMessage);
            return;
        }

        const uiMessage = new UiMessage();
        uiMessage.message = "No podemos continuar con el proceso de inicio de sesión, por favor intenta nuevamente más tarde.";
        uiMessage.type = MessageType.ERROR;
        showUiMessage(uiMessage);
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <section className={props.className}>
            <div className={"login-form-content"}>
                <div className={"login-form-close-button"}
                     onClick={props.onClose}>
                    <svg className={"icon-very-small"} data-name="Layer 1" xmlns="http://www.w3.org/2000/svg"
                         viewBox="0 0 500 500">
                        <path
                            d="M490.64,442.69l-193.1-193.1L490.64,56.49c12.79-13.24,12.42-34.34-.82-47.13-12.92-12.48-33.4-12.48-46.31,0l-193.1,193.1L57.31,9.36C44.07-3.43,22.97-3.07,10.18,10.18c-12.48,12.92-12.48,33.4,0,46.31l193.1,193.1L10.18,442.69c-13.24,12.79-13.61,33.89-.82,47.13,12.79,13.24,33.89,13.61,47.13.82.28-.27.55-.54.82-.82l193.1-193.1,193.1,193.1c13.24,12.79,34.34,12.42,47.13-.82,12.48-12.92,12.48-33.4,0-46.31Z"/>
                    </svg>
                </div>
                <h1 className={"text-3lvl"}>Iniciar sesión</h1>
                <input type={"email"}
                       placeholder={"Correo electrónico"}
                       value={email} onChange={(e) => setEmail(e.target.value)}
                       disabled={isFetchingApi || codeSent}
                />
                {codeSent && (
                    <input type={"text"}
                           placeholder={"Código de verificación"}
                           value={code}
                           onChange={(e) => setCode(e.target.value)}
                           disabled={isFetchingApi}
                    />
                )}
                {
                    isFetchingApi && (<div className="loader"></div>)
                }
                {
                    !isFetchingApi && (
                        <button className={`login-form-submit-button ${isFetchingApi || !email ? 'disable-button' : ''}`}
                                onClick={handleStartLoginProcess}
                        >
                            Siguiente
                        </button>
                    )
                }
                {loginMessage && <p className={loginClass}>{loginMessage}</p>}
            </div>
        </section>
    )
}

export default LoginCard;