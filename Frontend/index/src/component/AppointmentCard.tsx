import type {Appointment} from "../types/SystemTypes.ts";
import "./AppointmentCard.css";


interface Props {
    appointment: Appointment
}

function AppointmentCard(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className="appointment-card">
            <h3>{props.appointment?.startTime.getHours().toString()}:{props.appointment?.startTime.getMinutes().toString()}</h3>
            <p>{props.appointment?.vet?.name.toString()}</p>
        </div>
    );
}

export default AppointmentCard;