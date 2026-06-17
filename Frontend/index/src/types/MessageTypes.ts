// ---------------------------------------------------------------------------------------------------------------------
export const MessageType = {
    INFO: 'information',
    WARNING: 'warning',
    ERROR: 'error',
    SUCCESS: 'success'
} as const;

export type MessageType = typeof MessageType[keyof typeof MessageType];

// ---------------------------------------------------------------------------------------------------------------------
export const MessageDuration = {
    SHORT: 0,
    MEDIUM: 5,
    LONG: 10,
    VERY_LONG: 20,
    INTERMINABLE: 30
} as const;

export type MessageDuration = typeof MessageDuration[keyof typeof MessageDuration];

// ---------------------------------------------------------------------------------------------------------------------
export const MessageMood = {
    HAPPY: "HAPPY",
    SAD: "SAD",
    PLAYFUL: "PLAYFUL",
    HUNGRY: "HUNGRY",
    SLEEPY: "SLEEPY",
    EXCITED: "EXCITED",
    CALM: "CALM",
    CHILL: "CHILL",
    CONFIDENT: "CONFIDENT",
    ANGRY: "ANGRY",
    CRAZY: "CRAZY"
} as const;

export type MessageMood = typeof MessageMood[keyof typeof MessageMood];

// ---------------------------------------------------------------------------------------------------------------------
export class Message {
    public code: string | null;
    public message: string | null;
    status: number | null = null;
    public type: MessageType;
    public component: string | null;
    public id: number | null;

    // ------------------------------------------------------------------------
    constructor(
        code: string | null = null,
        message: string | null = null,
        status: number | null = null,
        type: MessageType = MessageType.INFO,
        component: string | null = null,
    ) {
        this.code = code;
        this.message = message;
        this.status = status;
        this.type = type;
        this.component = component;
        this.id = Date.now() + Math.random();
    }
}

// ---------------------------------------------------------------------------------------------------------------------
export class UiMessage extends Message {
    public duration: MessageDuration | null;
    public dismissible: boolean;
    public mood: MessageMood | null;

    // ------------------------------------------------------------------------
    constructor(
        component: string | null = null,
        code: string | null = null,
        message: string | null = null,
        status: number | null = null,
        type: MessageType = MessageType.INFO,
        duration: MessageDuration = MessageDuration.MEDIUM,
        dismissible: boolean = true,
        mood: MessageMood = MessageMood.HAPPY
    ) {
        super(code, message, status, type, component);
        this.duration = duration;
        this.dismissible = dismissible;
        this.mood = mood;
    }
}