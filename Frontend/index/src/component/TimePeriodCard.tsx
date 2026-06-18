import "./TimePeriodCard.css";
import {type TimePeriod} from "../types/SystemTypes.ts";
import AppointmentCard from "./AppointmentCard.tsx";

interface Props {
    timePeriod: TimePeriod
}


function TimePeriodCard(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <div className="time-period-card">
            <h2 className={"time"}>{props.timePeriod.startTime.getHours().toString()}</h2>
            <div className={"appointments"}>
                {props.timePeriod.appointments.map((appointment) => (
                    <AppointmentCard key={appointment.id} appointment={appointment}/>
                ))}
            </div>
        </div>
    );
}

export default TimePeriodCard;