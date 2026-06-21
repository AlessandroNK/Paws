import type {Appointment} from "../types/SystemTypes.ts";
import "./AppointmentCard.css";
import * as React from "react";


interface Props {
    appointment: Appointment,
    onAppointmentClick: (e: React.MouseEvent, appointment: Appointment) => Promise<void>
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
        hours = hours ? hours : 12;
        const minutesStr = minutes < 10 ? '0' + minutes : minutes.toString();
        return `${hours}:${minutesStr} ${ampm}`;
    }

    // -----------------------------------------------------------------------------------------------------------------
    async function handleClick(e: React.MouseEvent<HTMLDivElement>) {
        await props.onAppointmentClick(e, props.appointment)
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className={"appointment-card"}
             onClick={handleClick}
        >
            <h3>{to12HourFormat(props.appointment?.startTime)}</h3>
            <p>{props.appointment?.vet?.name.toString()}</p>
        </div>
    );
}

export default AppointmentCard;