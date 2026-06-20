import type {Appointment} from "../types/SystemTypes.ts";
import "./AppointmentCard.css";


interface Props {
    appointment: Appointment,
    onAppointmentClick: (appointment: Appointment) => void
}

function AppointmentCard(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------

    // Functions
    // -----------------------------------------------------------------------------------------------------------------
    const to12HourFormat = (date: Date): string => {
        let hours = date.getHours();
        const minutes = date.getMinutes();
        const ampm = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12;
        hours = hours ? hours : 12; // the hour '0' should be '12'
        const minutesStr = minutes < 10 ? '0' + minutes : minutes.toString();
        return `${hours}:${minutesStr} ${ampm}`;
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className="appointment-card"
            // onClick={() => props.onClick?.(props.appointment)}
             onClick={() => props.onAppointmentClick(props.appointment)}
        >
            <h3>{to12HourFormat(props.appointment?.startTime)}</h3>
            <p>{props.appointment?.vet?.name.toString()}</p>
        </div>
    );
}

export default AppointmentCard;