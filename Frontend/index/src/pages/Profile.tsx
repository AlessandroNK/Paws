import "./Profile.css";
import {Result} from "../types/CommonTypes.ts";
import {MessageDuration, MessageMood, MessageType, UiMessage} from "../types/MessageTypes.ts";
import {useEffect, useRef, useState} from "react";
import * as UserService from "../services/UserService.ts";
import * as HelperFunctions from "../resources/HelperFunctions.ts";
import * as PetService from "../services/PetService.ts";
import {User} from "../types/SystemTypes.ts";
import MenuBar from "../component/MenuBar.tsx";
import PetCard from "../component/PetCard.tsx";
import LoadingScreen from "../component/LoadingScreen.tsx";
import PawsChat from "../component/PawsChat.tsx";

function Profile() {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const isFetchingApi = useRef(false);
    const [pushMessages, setPushMessages] = useState([] as UiMessage[])
    const [user, setUser] = useState<User | null>(null);
    const userObject = useRef(user);
    const [loaded, setLoaded] = useState(false);
    const [pawsMessage, setPawsMessage] = useState<string>("");


    // Functions
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
    useEffect(() => {
        // User and log-in
        async function loadUser() {
            // Get local session
            const localSessionResult = UserService.getLocalSession();
            if (!localSessionResult.success || !localSessionResult.data) {
                HelperFunctions.clearLocalUserSession();
                userObject.current = null;
                setUser(userObject.current);
                return;
            }

            // Then check in the API
            userObject.current = localSessionResult.data;
            const userResult = await UserService.getApiSessionAsync(userObject.current);
            if (userResult.code === 'NETWORK_ERROR') {
                setUser(userObject.current);
                return;
            }

            if (!userResult.success) HelperFunctions.clearLocalUserSession();
            userObject.current = userResult.data;
            setUser(userObject.current);
        }

        // UI texts
        async function loadTranslation() {
        }

        // User's pets
        async function getUserPets() {
            if (!userObject.current) return;

            const petsResult = await PetService.getPetsByOwnerApi(userObject.current.id);
            if (!petsResult.success) return;
            if (userObject.current && petsResult.data) userObject.current.pets = petsResult.data;
            setUser(userObject.current);
        }

        function finalizeLoading() {
            setLoaded(true);
        }

        async function loadAllData() {
            // Change page name
            document.title = "PAWS 🐾 - Mi Perfil";

            if (isFetchingApi.current) return;
            isFetchingApi.current = true;

            await loadUser();
            await loadTranslation();
            await getUserPets();
            finalizeLoading();
            isFetchingApi.current = false;
        }

        loadAllData();
    }, []);

    // -----------------------------------------------------------------------------------------------------------------
    async function onUserProfileClick() {
    }

    // -----------------------------------------------------------------------------------------------------------------
    async function getPawsMessage() {
        const accessPoint = "chat";
        const url = HelperFunctions.createUrlRequest(accessPoint);

        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                UserName: userObject.current?.name || "Usuario",
                message: "HOla carola"
            })
        });

        const reader = response.body.getReader();
        const decoder = new TextDecoder();

        let fullMessage = "";
        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            fullMessage += decoder.decode(value);
            setPawsMessage(fullMessage);
        }
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    if (!loaded) return (<LoadingScreen/>);

    // -----------------------------------------------------------------------------------------------------------------
    return (
        <>
            <div className={"profile-page"}>
                <section className={"profile-header"}>
                    <MenuBar user={user} onUserProfileClick={onUserProfileClick}/>
                </section>

                <section className={"profile-body"}>
                    <div className={"profile-picture-container"}>
                        <img src={`/profiles/id${user?.id}.jpg`}
                             alt="Profile Picture"
                             className="profile-picture"/>
                    </div>

                    <div className={"profile-main-info"}>
                        <h2>Bienvenido, {user?.name}!</h2>
                        <p>{user?.email}</p>
                    </div>
                </section>

                <section className={"profile-pets"}>
                    <h2>Mis Mascotas</h2>
                    <button className={"add-pet-button"}
                            onClick={() => {
                                window.location.href = "/add-pet";
                            }}>Agregar Mascota
                    </button>


                    <div className={"pet-list"}>
                        {user?.pets && user.pets.length > 0 ? (
                            user.pets.map((pet) => (
                                <PetCard key={pet.id} pet={pet}/>
                            ))
                        ) : (
                            <p>No tienes mascotas registradas.</p>
                        )}
                    </div>
                </section>

                <PawsChat user={user}/>
            </div>
        </>
    )
}

export default Profile