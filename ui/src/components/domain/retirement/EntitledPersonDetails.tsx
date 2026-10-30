import { Calendar, FileText, Hash, Phone, UserCircle2 } from "lucide-react";

import { Badge, EditableImage, Heading } from "@/components/ui";
import { formatDate, getDocumentUrl, resolvePersonalPhotoPath } from "@/lib/utils";

interface EntitledPerson {
    entitledID?: number;
    fullName: string;
    nationalID: string;
    relationshipName?: string;
    dateOfBirth?: string;
    ageYears?: number;
    phone?: string;
    socialStatusName?: string;
    healthStatusName?: string;
    isEligible: boolean;
    stopReason?: string;
    stopCode?: string;
    personalPhotoPath?: string | null;
}

const noopUpload = async (_file: File) => {};

export function EntitledPersonDetails({ 
    person,
    onPhotoUpload 
}: { 
    person: EntitledPerson;
    onPhotoUpload?: (file: File) => Promise<void>;
}) {
    const getPhotoUrl = (path?: string | null) => {
        if (!path) return null;
        if (path.startsWith("http")) return path;
        return getDocumentUrl(path);
    };

    return (
        <div className="space-y-3 lg:space-y-4 text-start">
<<<<<<< HEAD
            <div className="bg-gradient-to-l from-primary/5 to-primary/10 rounded-xl p-3 lg:p-5 border border-primary relative overflow-hidden">
                <div className="absolute top-0 end-0 p-8 opacity-100 pointer-events-none">
=======
            <div className="bg-gradient-to-l from-primary/5 to-primary/10 rounded-xl p-3 lg:p-5 border border-primary/20 relative overflow-hidden">
                <div className="absolute top-0 end-0 p-8 opacity-[0.03] pointer-events-none">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <UserCircle2 className="w-32 lg:w-48 h-32 lg:h-48 max-w-[40vw] sm:max-w-none" />
                </div>

                <div className="relative flex flex-col sm:flex-row gap-3 lg:gap-4 items-start">
                    <div className="flex-shrink-0">
                        <EditableImage
                            src={getPhotoUrl(resolvePersonalPhotoPath(person))}
                            alt={person.fullName}
                            fallbackIcon={<UserCircle2 className="w-12 lg:w-16 h-12 lg:h-16 text-muted-foreground" />}
                            className="w-16 lg:w-20 h-16 lg:h-20 rounded-xl border-4 border-white"
                            onUpload={onPhotoUpload || noopUpload}
                            canEdit={!!onPhotoUpload}
                        />
                    </div>

                    <div className="flex-1 space-y-1.5 lg:space-y-2 min-w-0">
                        <div>
                            <Heading level="h2" className="text-base lg:text-xl font-bold text-foreground tracking-tight break-words">{person.fullName}</Heading>
                            <p className="text-xs lg:text-sm text-muted-foreground mt-1 font-bold">{person.relationshipName}</p>
                        </div>
                        <div className="flex flex-wrap gap-2 lg:gap-3">
                            <span className="text-[10px] lg:text-xs text-muted-foreground font-bold">رقم الهوية: {person.nationalID || "—"}</span>
                            {person.ageYears && <span className="text-[10px] lg:text-xs text-muted-foreground font-bold">{person.ageYears} سنة</span>}
                        </div>
                    </div>

                    <Badge variant={person.isEligible ? "success" : "danger"} className="text-xs lg:text-sm px-2.5 lg:px-3 py-0.5 lg:py-1 font-bold shrink-0">
                        {person.isEligible ? "مستحق للصرف" : "موقوف"}
                    </Badge>
                </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 lg:gap-4">
                <div className="bg-card border rounded-xl lg:rounded-lg p-3 lg:p-5 space-y-2 lg:space-y-3">
                    <Heading level="h3" className="text-sm lg:text-lg font-semibold flex items-center gap-2 text-primary border-b pb-2"><UserCircle2 className="w-4 h-4 lg:w-5 lg:h-5 shrink-0" /> البيانات الشخصية</Heading>
                    <div className="space-y-2 lg:space-y-3">
                        <div className="flex items-center gap-2 lg:gap-3">
                            <Hash className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground flex-shrink-0" />
                            <div className="flex-1 flex items-center justify-between gap-2">
                                <p className="text-[10px] lg:text-xs text-muted-foreground font-bold shrink-0">رقم الهوية الوطنية</p>
                                <p className="text-xs lg:text-sm font-mono font-bold tracking-wider break-all text-end">{person.nationalID || "بدون رقم وطني"}</p>
                            </div>
                        </div>
                        <div className="flex items-center gap-2 lg:gap-3">
                            <Calendar className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground flex-shrink-0" />
                            <div className="flex-1 flex items-center justify-between gap-2">
                                <p className="text-[10px] lg:text-xs text-muted-foreground font-bold shrink-0">تاريخ الميلاد</p>
                                <p className="text-xs lg:text-sm font-bold break-words text-end">{formatDate(person.dateOfBirth)} {person.ageYears ? <span className="text-[10px] lg:text-xs text-muted-foreground">({person.ageYears} سنة)</span> : null}</p>
                            </div>
                        </div>
                        {person.phone && (
                            <div className="flex items-center gap-2 lg:gap-3">
                                <Phone className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground flex-shrink-0" />
                                <div className="flex-1 flex items-center justify-between gap-2">
                                    <p className="text-[10px] lg:text-xs text-muted-foreground font-bold shrink-0">رقم الهاتف</p>
<<<<<<< HEAD
                                    <p className="text-xs lg:text-sm text-end">{person.phone}</p>
=======
                                    <p className="text-xs lg:text-sm font-mono font-bold text-end">{person.phone}</p>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                </div>
                            </div>
                        )}
                    </div>
                </div>

                <div className="bg-card border rounded-xl lg:rounded-lg p-3 lg:p-5 space-y-2 lg:space-y-3">
                    <Heading level="h3" className="text-sm lg:text-lg font-semibold flex items-center gap-2 text-primary border-b pb-2"><FileText className="w-4 h-4 lg:w-5 lg:h-5 shrink-0" /> الحالة والارتباط</Heading>
                    <div className="space-y-2 lg:space-y-3">
                        <div className="flex items-center gap-2 lg:gap-3">
                            <UserCircle2 className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground flex-shrink-0" />
                            <div className="flex-1 flex items-center justify-between gap-2">
                                <p className="text-[10px] lg:text-xs text-muted-foreground font-bold shrink-0">صلة القرابة</p>
                                <p className="text-xs lg:text-sm font-bold text-end">{person.relationshipName}</p>
                            </div>
                        </div>
                        <div className="flex items-center gap-2 lg:gap-3">
                            <FileText className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground flex-shrink-0" />
                            <div className="flex-1 flex items-center justify-between gap-2">
                                <p className="text-[10px] lg:text-xs text-muted-foreground font-bold shrink-0">الحالة الاجتماعية</p>
                                <p className="text-xs lg:text-sm font-bold text-end">{person.socialStatusName || "غير محدد"}</p>
                            </div>
                        </div>
                        <div className="flex items-center gap-2 lg:gap-3">
                            <FileText className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground flex-shrink-0" />
                            <div className="flex-1 flex items-center justify-between gap-2">
                                <p className="text-[10px] lg:text-xs text-muted-foreground font-bold shrink-0">الحالة الصحية</p>
                                <p className="text-xs lg:text-sm font-bold text-end">{person.healthStatusName || "غير محدد"}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {person.stopReason && (
                <div className="bg-card border rounded-xl lg:rounded-lg p-3 lg:p-5 space-y-2 lg:space-y-3">
                    <Heading level="h3" className="text-sm lg:text-lg font-semibold flex items-center gap-2 text-primary border-b pb-2"><FileText className="w-4 h-4 lg:w-5 lg:h-5 shrink-0" /> مبررات الحالة</Heading>
<<<<<<< HEAD
                    <div className="bg-card/80 p-2.5 lg:p-4 rounded-xl border border-muted italic text-xs lg:text-sm font-bold text-foreground leading-relaxed">"{person.stopReason}"</div>
=======
                    <div className="bg-card/80 p-2.5 lg:p-4 rounded-xl border border-muted/30 italic text-xs lg:text-sm font-bold text-foreground/80 leading-relaxed">"{person.stopReason}"</div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                </div>
            )}
        </div>
    );
}
