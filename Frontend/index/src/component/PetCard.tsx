import "./PetCard.css"
import {type Pet} from "../types/SystemTypes.ts";
import AnimalIcon from "./AnimalIcon.tsx";

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
            </div>
        </div>
    )
}

export default PetCard;