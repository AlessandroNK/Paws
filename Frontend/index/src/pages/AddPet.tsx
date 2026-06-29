import "./AddPet.css"
import {useEffect, useRef, useState} from "react";
import * as UserService from "../services/UserService.ts";
import * as HelperFunctions from "../resources/HelperFunctions.ts";
import {Pet, PetSpecies, User} from "../types/SystemTypes.ts";
import MenuBar from "../component/MenuBar.tsx";
import LoadingScreen from "../component/LoadingScreen.tsx";
import AnimalIcon from "../component/AnimalIcon.tsx";

function AddPet() {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const [user, setUser] = useState<User | null>(null);
    const [loaded, setLoaded] = useState(false);
    const userObject = useRef(user);
    const isFetching = useRef(false);
    const [loginMessage, setLoginMessage] = useState<string>("");
    const [loginClass, setLoginClass] = useState<string>("");

    // -----------------------------------------------------------------------------------------------------------------
    const [pet, setPet] = useState<Pet | null>(null);
    const petObject = useRef(pet);
    const [petName, setPetName] = useState<string>("");
    const [petSpecies, setPetSpecies] = useState<PetSpecies>(PetSpecies.Dog);
    const [petBreed, setPetBreed] = useState<string | null>(null);


    // Functions
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

            if (!userResult.success) HelperFunctions.clearLocalUserSession();
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

            await loadUser();
            await loadTranslation();
            finalizeLoading();
        }

        loadAllData();
    }, []);

    // -----------------------------------------------------------------------------------------------------------------
    function handleAddPet() {
        if (isFetching.current) return;

        if (!userObject.current) {
            setLoginMessage("Usuario no autenticado. Por favor, inicia sesión.");
            setLoginClass("error-message");
            return;
        }

        if (!petName.trim()) {
            setLoginMessage("Por favor, ingresa un nombre para la mascota.");
            setLoginClass("error-message");
            return;
        }

        isFetching.current = true;
        const newPet = new Pet(
            0,
            petName.trim(),
            petSpecies,
            petBreed?.trim() || null
        );

        // Here you would typically send the newPet object to your backend API to save it.
        // For now, we'll just log it to the console.
        console.log("New Pet:", newPet);

        // Reset form fields after adding the pet
        isFetching.current = false;
        setPetName("");
        setPetSpecies(PetSpecies.Dog);
        setPetBreed(null);
        setLoginMessage("Mascota agregada exitosamente.");
        setLoginClass("success-message");
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
                    <input className={"form-input"}
                           type="text"
                           placeholder="Nombre de la mascota"
                           value={petName}
                           autoComplete={"off"}
                           onChange={(e) => setPetName(e.target.value)}
                    />
                    <div className={"flex row gap-3 w-100"}>
                        <AnimalIcon className={"w-12 h-12 fill-main-soft"} species={petSpecies}/>
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
                    </div>

                    <input className={"form-input"}
                           type="text"
                           placeholder="Raza de la mascota (opcional)"
                           value={petBreed || ""}
                           autoComplete={"off"}
                           onChange={(e) => setPetBreed(e.target.value)}
                    />
                    <button className={"form-button"}
                            onClick={handleAddPet}>
                        Agregar Mascota
                    </button>
                    <p className={loginClass}>{loginMessage}</p>
                </div>
            </section>
        </div>
    );
}

export default AddPet;