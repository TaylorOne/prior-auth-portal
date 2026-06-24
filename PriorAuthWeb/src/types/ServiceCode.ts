export interface ServiceCode {
    code: string;
    codeSystem: string;
    displayName: string;
    requiredSpecialty?: string | null;
}