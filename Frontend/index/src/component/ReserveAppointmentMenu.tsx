import "./ReserveAppointmentMenu.css";
import {type Appointment, User} from "../types/SystemTypes.ts";
import {useState} from "react";
import AnimalIcon from "./AnimalIcon.tsx";

interface Props {
    appointment: Appointment | null,
    user: User | null,
    onClose: () => void,
    onReserve: (petId: number) => Promise<void>,
    mousePosition: { x: number, y: number },
    loading: boolean,
}

function ReserveAppointmentMenu(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const [selectedPet, setSelectedPet] = useState<number | null>(null);
    const [loading, setIsloading] = useState<boolean>(false);

    // Functions
    // -----------------------------------------------------------------------------------------------------------------
    function createPetList() {
        if (!props.user) return null;
        return props.user.pets.map((pet, index) => (
            <div key={index}
                 className={`pet-item ${selectedPet === pet.id ? "selected" : ""}`}
                 onClick={(e) => {
                     e.stopPropagation();
                     setSelectedPet(pet.id);
                 }}
            >
                <AnimalIcon species={pet.species} className={"icon-small"}/>
                <span className="pet-name">{pet.name}</span>
            </div>
        ));
    }

    // -----------------------------------------------------------------------------------------------------------------
    async function handleReserve(e: React.MouseEvent<HTMLButtonElement>) {
        e.preventDefault();
        e.stopPropagation();

        if (selectedPet === null) return
        setIsloading(true);
        await props.onReserve(selectedPet);
        setIsloading(false);

    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    if (!props.appointment || !props.user) return null;

    // -----------------------------------------------------------------------------------------------------------------
    return (
        <>
            <div className={"reserve-appointment-menu-overlay"} onClick={props.onClose}></div>
            <div className="reserve-appointment-menu"
                 style={{
                     position: 'fixed',
                     top: props.mousePosition.y - 180,
                     left: props.mousePosition.x + 20,
                 }}
                 onClick={(e: React.MouseEvent<HTMLDivElement>) => {
                     e.preventDefault();
                     e.stopPropagation();
                     setSelectedPet(null)
                 }}
            >
                <h2 className={"reserve-title text-5lvl"}>Para quién es lacita?</h2>
                {createPetList()}
                {loading && <div className="reserve-action loader"></div>}
                {!loading && (<button className={"reserve-action reserve-button" + (selectedPet === null ? " disabled" : "")}
                                      onClick={handleReserve}>
                    Reservar
                </button>)}
            </div>
        </>
    );
}

export default ReserveAppointmentMenu;