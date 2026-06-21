import "./ReserveAppointmentMenu.css";
import {type Appointment, PetSpecies, User} from "../types/SystemTypes.ts";
import {useState} from "react";

interface Props {
    appointment: Appointment | null,
    user: User | null,
    onClose: () => void,
    mousePosition: { x: number, y: number }
}

function ReserveAppointmentMenu(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const [selectedPet, setSelectedPet] = useState<number | null>(null);

    // Functions
    // -----------------------------------------------------------------------------------------------------------------
    function speciesToEmoji(species: number): string {
        switch (species) {
            case PetSpecies.Other:
                return "🐾";
            case PetSpecies.Dog:
                return "🐶";
            case PetSpecies.Cat:
                return "🐱";
            case PetSpecies.Bunny:
                return "🐰";
            case PetSpecies.Hamster:
                return "🐹";
            case PetSpecies.Turtle:
                return "🐢";
            case PetSpecies.Cow:
                return "🐮";
            case PetSpecies.Horse:
                return "🐴";
            case PetSpecies.Bird:
                return "🐦";
            case PetSpecies.Fish:
                return "🐟";
            case PetSpecies.Reptile:
                return "🦎";
            case PetSpecies.Rodent:
                return "🐀";
            default:
                return "❓";
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    function createPetList() {
        if (!props.user) return null;
        return props.user.pets.map((pet, index) => (
            <div key={index} className="pet-item">
                <span className="pet-type">{speciesToEmoji(pet.species)}</span>
                <span className="pet-name">{pet.name}</span>
            </div>
        ));
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <>
            <div className={"reserve-appointment-menu-overlay"} onClick={props.onClose}></div>
            <div className="reserve-appointment-menu"
                 style={{
                     position: 'fixed',
                     top: props.mousePosition.y - 150,
                     left: props.mousePosition.x,
                 }}
            >
                <h2 className={"text-4lvl"}>Para quién es la <span className={"highlight-text"}>cita</span>?</h2>
                {createPetList()}
                <button className={"reserve-button" + (selectedPet === null ? " disabled" : "")}
                        onClick={() => alert("Funcionalidad de reserva aún no implementada")}>
                    Reservar
                </button>
            </div>
        </>
    );
}

export default ReserveAppointmentMenu;