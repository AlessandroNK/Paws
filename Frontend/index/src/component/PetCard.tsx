import "./PetCard.css"
import {type Pet} from "../types/SystemTypes.ts";
import AnimalIcon from "./AnimalIcon.tsx";
import AppointmentPetCard from "./AppointmentPetCard.tsx";

interface Props {
    pet: Pet
}

function PetCard(props: Props) {
    return (
        <div className="pet-card">
            <div className="pet-image">
                <AnimalIcon className="pet-icon" species={props.pet.species}/>
            </div>
            <div className="pet-details">
                <h3 className="pet-name">{props.pet.name}</h3>
                <p>{props.pet.breed}</p>
                <div className={"pet-appointments"}>
                    <h4> citas programadas </h4>
                    {props.pet.appointments.length > 0 ? (
                        <div className={"pet-appointments-list"}>
                            {props.pet.appointments.map((appointment, index) => (
                                <AppointmentPetCard key={index} appointment={appointment}/>
                            ))}
                        </div>
                    ) : (
                        <p>No tiene citas programadas.</p>
                    )}
                </div>
            </div>
        </div>
    )
}

export default PetCard;