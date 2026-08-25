import api from "@/api/axios";
import type {
  CorrespondenceListResponse,
  CorrespondenceTypeListResponse,
} from "@/types/correspondence";

export const directorApi = {
  getRecentCorrespondence: (params: { direction?: number; page?: number; pageSize?: number }) =>
    api.get<CorrespondenceListResponse>("/Correspondence", { params }).then((r) => r.data),

  getTypes: () =>
    api.get<CorrespondenceTypeListResponse>("/Correspondence/types").then((r) => r.data),
};
