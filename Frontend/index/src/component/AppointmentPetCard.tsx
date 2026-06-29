import "./AppointmentPetCard.css";
import type {Appointment} from "../types/SystemTypes.ts";

interface Props {
    appointment: Appointment;
}

function AppointmentPetCard(props: Props) {
    if (!props.appointment) {
        return <div className="appointment-pet-card">No appointment data available.</div>;
    }

    // -----------------------------------------------------------------------------------------------------------------
    if (props.appointment.startTime < new Date()) return;

    // -----------------------------------------------------------------------------------------------------------------

    return (
        <div className="appointment-pet-card">
            <div className="appointment-date">
                {props.appointment.startTime.toLocaleDateString()} - {props.appointment.startTime.toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit'
            })}
            </div>
            <div className={"appointments-pet-close-button"}
                 onClick={props.onClose}>
                <svg className={"icon-very-small"} xmlns="http://www.w3.org/2000/svg" viewBox="0 0 333.33 200">
                    <g id="Arrow_Caret_Down_MD" data-name="Arrow / Caret_Down_MD">
                        <g id="Vector">
                            <path
                                d="M166.67,200c-8.84,0-17.32-3.51-23.57-9.76L9.76,56.9C-3.25,43.89-3.25,22.78,9.76,9.76s34.12-13.02,47.14,0l109.76,109.76L276.43,9.76c13.02-13.02,34.12-13.02,47.14,0,13.02,13.02,13.02,34.12,0,47.14l-133.33,133.33c-6.25,6.25-14.73,9.76-23.57,9.76Z"/>
                        </g>
                    </g>
                </svg>
            </div>
        </div>
    );
}

export default AppointmentPetCard;