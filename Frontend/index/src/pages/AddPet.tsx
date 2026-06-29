import "./AddPet.css"
import {useEffect, useRef, useState} from "react";
import * as UserService from "../services/UserService.ts";
import * as HelperFunctions from "../resources/HelperFunctions.ts";
import {Pet, PetSpecies, User} from "../types/SystemTypes.ts";
import MenuBar from "../component/MenuBar.tsx";
import LoadingScreen from "../component/LoadingScreen.tsx";
import AnimalIcon from "../component/AnimalIcon.tsx";
import * as PetService from "../services/PetService.ts";
import {MessageDuration, MessageMood, MessageType, UiMessage} from "../types/MessageTypes.ts";
import type {Result} from "../types/CommonTypes.ts";
import PushMessagesUi from "../component/PushMessagesUi.tsx";

function AddPet() {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const [user, setUser] = useState<User | null>(null);
    const [loaded, setLoaded] = useState(false);
    const userObject = useRef(user);
    const isFetchingApi = useRef(false);
    const [loginMessage, setLoginMessage] = useState<string>("");
    const [loginClass, setLoginClass] = useState<string>("");

    // -----------------------------------------------------------------------------------------------------------------
    const [pushMessages, setPushMessages] = useState([] as UiMessage[])
    const [petName, setPetName] = useState<string>("");
    const [petSpecies, setPetSpecies] = useState<PetSpecies>(PetSpecies.Dog);
    const [petBreed, setPetBreed] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(false);


    // Functions
    // -----------------------------------------------------------------------------------------------------------------
    useEffect(() => {
        // User and log-in
        async function loadUser() {
            // Get local session
            const localSessionResult = UserService.getLocalSession();
            if (!localSessionResult.success || !localSessionResult.data) {
                console.log("E1")
                HelperFunctions.clearLocalUserSession();
                userObject.current = null;
                setUser(userObject.current);
                setLoaded(true);
                return;
            }

            // Then check in the API
            userObject.current = localSessionResult.data;
            const userResult = await UserService.getApiSessionAsync(userObject.current);
            if (userResult.code === 'NETWORK_ERROR') {
                setUser(userObject.current);
                setLoaded(true);
                return;
            }

            if (!userResult.success) {
                console.log("E2")

                HelperFunctions.clearLocalUserSession();
            }
            userObject.current = userResult.data;
            setUser(userObject.current);
            setLoaded(true);
        }

        // UI texts
        async function loadTranslation() {
        }

        function finalizeLoading() {
            setLoaded(true);
        }

        async function loadAllData() {
            // Change page name
            document.title = "PAWS 🐾 - Agregar Mascota";

            if (isFetchingApi.current) return;
            isFetchingApi.current = true;

            await loadUser();
            await loadTranslation();
            finalizeLoading();
            isFetchingApi.current = false;
        }

        loadAllData();
    }, []);

    // -----------------------------------------------------------------------------------------------------------------
    async function showErrorsAsPushMessages(result: Result<unknown>) {
        const pushMessages: UiMessage[] = [];
        if (result.code) {
            const uiMessage = new UiMessage();
            uiMessage.type = result.success ? MessageType.SUCCESS : MessageType.ERROR;
            uiMessage.mood = result.success ? MessageMood.HAPPY : MessageMood.SAD;
            uiMessage.duration = MessageDuration.MEDIUM;
            uiMessage.component = result.component;
            uiMessage.code = result.code;
            pushMessages.push(uiMessage);
        }

        if (result.errors.length > 0) {
            result.errors.forEach(error => {
                for (const message of error.messages) {
                    pushMessages.push(message);
                }
            });
        }
        setPushMessages(pushMessages);
    }

    // -----------------------------------------------------------------------------------------------------------------
    async function handleAddPet() {
        if (isFetchingApi.current) return;

        if (!userObject.current) {
            setLoginMessage("Usuario no autenticado. Por favor, inicia sesión.");
            setLoginClass("standard-ui-error-message");
            return;
        }

        if (!petName.trim()) {
            setLoginMessage("Por favor, ingresa un nombre para la mascota.");
            setLoginClass("standard-ui-error-message");
            return;
        }

        const newPet = new Pet(
            0,
            petName.trim(),
            petSpecies,
            petBreed?.trim() || null
        );

        // Add pet
        setLoginMessage("");
        setIsLoading(true);
        isFetchingApi.current = true;
        const result = await PetService.addPetApi(
            userObject.current.id,
            newPet
        );
        setIsLoading(false);
        isFetchingApi.current = false;

        // Show errors up
        showErrorsAsPushMessages(result);

        // Process result
        if (!result.success) {
            setLoginMessage("Error al agregar la mascota. Por favor, intenta nuevamente.");
            setLoginClass("standard-ui-error-message");
            return;
        }

        // Redirect to profile page after adding the pet
        window.location.href = "/profile";
    }

    // -----------------------------------------------------------------------------------------------------------------
    const onUserProfileClick = () => window.location.href = "/profile";

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    if (!loaded) return (<LoadingScreen/>);

    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className={"add-pet-page"}>
            <section className={"profile-header"}>
                <MenuBar user={user} onUserProfileClick={onUserProfileClick}/>
            </section>

            <section className={"add-pet-content"}>
                <h1>Agregar Mascota</h1>
                <p>Formulario para agregar una nueva mascota.</p>
            </section>

            <section className={"pet-form"}>
                <div className={"add-pet-form"}>
                    <div className={"add-pet-form-icon"}>
                        <AnimalIcon species={petSpecies} className={"h-60 w-60 fill-main"}/>
                    </div>
                    <div className={"add-pet-form-content" +
                        ""}>
                        <input className={"form-input"}
                               type="text"
                               placeholder="Nombre de la mascota"
                               value={petName}
                               autoComplete={"off"}
                               onChange={(e) => setPetName(e.target.value)}
                        />
                        <select
                            className="form-select"
                            value={petSpecies}
                            onChange={(e) => setPetSpecies(Number(e.target.value) as PetSpecies)}
                        >
                            <option value={PetSpecies.Dog}>Perro</option>
                            <option value={PetSpecies.Cat}>Gato</option>
                            <option value={PetSpecies.Bird}>Ave</option>
                            <option value={PetSpecies.Bunny}>Conejo</option>
                            <option value={PetSpecies.Hamster}>Hámster</option>
                            <option value={PetSpecies.Fish}>Pez</option>
                            <option value={PetSpecies.Reptile}>Reptil</option>
                            <option value={PetSpecies.Turtle}>Tortuga</option>
                        </select>

                        <input className={"form-input"}
                               type="text"
                               placeholder="Raza de la mascota (opcional)"
                               value={petBreed || ""}
                               autoComplete={"off"}
                               onChange={(e) => setPetBreed(e.target.value)}
                        />
                        {!isLoading && (
                            <button className={"form-button"}
                                    onClick={handleAddPet}>
                                Agregar Mascota
                            </button>
                        )}
                        {isLoading && (<div className={"loader-white"}></div>)}
                        <p className={loginClass}>{loginMessage}</p>
                    </div>
                </div>
            </section>
            <PushMessagesUi messages={pushMessages}/>
        </div>
    );
}

export default AddPet;