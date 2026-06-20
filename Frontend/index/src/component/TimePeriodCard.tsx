import "./TimePeriodCard.css";
import {Appointment, type TimePeriod} from "../types/SystemTypes.ts";
import AppointmentCard from "./AppointmentCard.tsx";

interface Props {
    timePeriod: TimePeriod,
    onAppointmentClick: (appointment: Appointment) => void
}


function TimePeriodCard(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const to12HourFormat = (date: Date): string => {
        let hours = date.getHours();
        const ampm = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12;
        hours = hours ? hours : 12;
        return `${hours} ${ampm}`;
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className="time-period-card">
            <h2 className={"time"}>{to12HourFormat(props.timePeriod.startTime)}</h2>
            <div className={"appointments"}>
                {props.timePeriod.appointments.map((appointment) => (
                    <AppointmentCard key={appointment.id} appointment={appointment} onAppointmentClick={props.onAppointmentClick}/>
                ))}
            </div>
        </div>
    );
}

export default TimePeriodCard;