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
        <div className="appointment-card"
            // onClick={() => props.onClick?.(props.appointment)}
             onClick={() => console.log("Clicked appointment with id: " + props.appointment.id)}
        >
            <h3>{props.appointment?.startTime.getHours().toString().padStart(2, "0")}:{props.appointment?.startTime.getMinutes().toString().padStart(2, "0")}</h3>
            <p>{props.appointment?.vet?.name.toString()}</p>
        </div>
    );
}

export default AppointmentCard;