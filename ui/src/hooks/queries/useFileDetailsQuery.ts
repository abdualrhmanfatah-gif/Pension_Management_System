import { useQuery } from "@tanstack/react-query";

import { retireeApi } from "@/api/retirees";
import { retirementApi } from "@/api/retirement";

export interface RetirementFileDetail {
    reviewStatus: number;
    reviewNotes: any;
    salaryStatus?: string;
    pensionAmount?: number;
    fileID: number;
    fileNumber: string;
    retireeID: number;
    retireeFullName: string;
    nationalID: string | null;
    militaryNumber: string | null;
    rankName: string | null;
    branchName: string | null;
    fileStatusName: string | null;
    retirementDate: string | null;
    serviceStartDate: string | null;
    serviceEndDate: string | null;
    fileOpenDate?: string | null;
    closedDate?: string | null;
    statusReason?: string | null;
    notes: string | null;
    createdAt: string;
    updatedAt?: string | null;
    assignedEmployeeID?: number | null;
    assignedEmployeeName?: string | null;
    activePaymentBookNumber?: string | null;
    activePaymentBookExpiry?: string | null;
    activePaymentBookStatus?: string | null;
    birthDate?: string | null;
    gender?: number;
    address?: string | null;
    deathDate?: string | null;
    isAlive?: boolean;
    entitledPersonsCount?: number;
    agentsCount?: number;
    guardiansCount?: number;
    missingDocumentsCount?: number;
    missingDocumentsList?: string[];
    missingDataCount?: number;
    missingDataList?: string[];
    personalPhotoPath?: string | null;
    idDocumentPath?: string | null;
    rankID?: number | null;
    pensionBranchID?: number | null;
    statusID?: number | null;
    reviewCount?: number;
    hasMinors?: boolean;
    hasPledgesOrExemptions?: boolean;
    hasSettlements?: boolean;
    reviewRequestedAt?: string | null;
    reviewCompletedAt?: string | null;
    statusName?: string | null;
}

export interface RetireeDetail {
    birthDate: string;
    iban: string;
    bankID: number;
    retireeID: number;
    fullName: string;
    nationalID: string;
    militaryNumber: string | null;
    unitName?: string | null;
    categoryName?: string | null;
    gender: number;
    phone: string | null;
    address: string | null;
    city: string | null;
    governorate: string | null;
    isAlive: boolean;
    deathDate: string | null;
    socialStatusName: string | null;
    socialStatusID?: number;
    healthStatusName: string | null;
    healthStatusID?: number;
    personalPhotoPath?: string | null;
}

export const FILE_DETAIL_KEY = "retirement-file";

export function useFileDetailsQuery(fileId: number | undefined) {
    const fileQuery = useQuery({
        queryKey: [FILE_DETAIL_KEY, fileId],
        queryFn: async () => {
            const data = await retirementApi.getFile(fileId!);
            const d = data as Record<string, any>;
            
            const retireeId = data.retireeID ?? d.RetireeID;
            let r = null;
            if (retireeId) {
                r = await retireeApi.getById(retireeId);
            }
            
            // Normalize casing (Backend might return PascalCase or camelCase)
            const normalizedFile: RetirementFileDetail = {
                ...data,
                fileID: data.fileID ?? d.FileID,
                fileNumber: data.fileNumber ?? d.FileNumber,
                retireeID: data.retireeID ?? d.RetireeID,
                retireeFullName: data.retireeFullName ?? d.RetireeFullName,
                nationalID: data.nationalID ?? d.NationalID,
                rankName: data.rankName ?? d.RankName,
                branchName: data.branchName ?? d.BranchName,
                fileStatusName: data.fileStatusName ?? d.FileStatusName,
                retirementDate: data.retirementDate ?? d.RetirementDate,
                serviceStartDate: data.serviceStartDate ?? d.ServiceStartDate,
                serviceEndDate: data.serviceEndDate ?? d.ServiceEndDate,
                notes: data.notes ?? d.Notes,
                createdAt: data.createdAt ?? d.CreatedAt,
                updatedAt: data.updatedAt ?? d.UpdatedAt,
                missingDocumentsCount: data.missingDocumentsCount ?? d.MissingDocumentsCount ?? 0,
                missingDocumentsList: (data as any).missingDocumentsList ?? (data as any).MissingDocumentsList ?? [],
                missingDataCount: data.missingDataCount ?? d.MissingDataCount ?? 0,
                missingDataList: data.missingDataList ?? d.MissingDataList ?? [],
                hasMinors: data.hasMinors ?? d.HasMinors,
                hasPledgesOrExemptions: data.hasPledgesOrExemptions ?? d.HasPledgesOrExemptions,
                hasSettlements: data.hasSettlements ?? d.HasSettlements,
                birthDate: data.birthDate ?? d.BirthDate,
                isAlive: data.isAlive ?? d.IsAlive,
                entitledPersonsCount: data.entitledPersonsCount ?? d.EntitledPersonsCount ?? 0,
                agentsCount: data.agentsCount ?? d.AgentsCount ?? 0,
                guardiansCount: data.guardiansCount ?? d.GuardiansCount ?? 0,
                reviewStatus: data.reviewStatus ?? d.ReviewStatus ?? 0,
                reviewNotes: data.reviewNotes ?? d.ReviewNotes ?? null,
                reviewRequestedAt: (data as any).reviewRequestedAt ?? d.ReviewRequestedAt ?? null,
                reviewCompletedAt: (data as any).reviewCompletedAt ?? d.ReviewCompletedAt ?? null,
                salaryStatus: "",
                pensionAmount: 0
            };
            
            let retireeData: RetireeDetail | null = null;
            if (r) {
                retireeData = {
                    ...r,
                    retireeID: r.retireeID ?? r.RetireeID,
                    fullName: r.fullName ?? r.FullName,
                    nationalID: r.nationalID ?? r.NationalID,
                    militaryNumber: r.militaryNumber ?? r.MilitaryNumber,
                    gender: r.gender ?? r.Gender,
                    phone: r.phone ?? r.Phone,
                    address: r.address ?? r.Address,
                    city: r.city ?? r.City,
                    governorate: r.governorate ?? r.Governorate,
                    isAlive: r.isAlive ?? r.IsAlive,
                    birthDate: r.birthDate ?? r.BirthDate,
                    iban: r.iban ?? r.Iban ?? r.IBAN,
                    bankID: r.bankID ?? r.BankID,
                    socialStatusName: r.socialStatusName ?? r.SocialStatusName,
                    socialStatusID: r.socialStatusID ?? r.SocialStatusID,
                    healthStatusName: r.healthStatusName ?? r.HealthStatusName,
                    healthStatusID: r.healthStatusID ?? r.HealthStatusID,
                    personalPhotoPath: r.personalPhotoPath ?? r.PersonalPhotoPath,
                };
            }
            
            return { file: normalizedFile, retiree: retireeData };
        },
        enabled: !!fileId && fileId > 0,
        staleTime: 30_000,
    });
    
    return {
        file: fileQuery.data?.file ?? null,
        retiree: fileQuery.data?.retiree ?? null,
        isLoading: fileQuery.isLoading,
        isError: fileQuery.isError,
        error: fileQuery.error ?? null,
        refetch: fileQuery.refetch,
    };
}
