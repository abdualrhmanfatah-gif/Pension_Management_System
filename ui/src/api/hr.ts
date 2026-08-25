import api from "./axios";
import type { ApiResponse, PagedResult } from "@/types/api";

// ─── Reference Data ──────────────────────────────────────

export interface DepartmentDto {
  departmentID: number;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  employeesCount: number;
  sectionsCount: number;
}

export interface CreateDepartmentDto {
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface SectionDto {
  sectionID: number;
  departmentID: number;
  departmentName: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  employeesCount: number;
}

export interface CreateSectionDto {
  departmentID: number;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface JobTitleDto {
  jobTitleID: number;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  employeesCount: number;
}

export interface CreateJobTitleDto {
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface EmploymentStatusDto {
  employmentStatusID: number;
  code: string;
  name: string;
  isInService: boolean;
  employeesCount: number;
}

export interface LeaveTypeDto {
  leaveTypeID: number;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface RankDto {
  rankID: number;
  rankName: string;
  rankLevel: number;
  baseSalary: number;
}

// ─── Employees ───────────────────────────────────────────

export interface EmployeeListItemDto {
  employeeID: number;
  employeeNumber: string;
  fullName: string;
  nationalID: string;
  departmentName: string;
  sectionName?: string | null;
  jobTitleID: number;
  jobTitleName: string;
  employmentStatusName: string;
  profilePhotoPath?: string | null;
  isActive: boolean;
  hireDate: string;
}

export interface EmployeeDetailsDto extends EmployeeListItemDto {
  departmentID: number;
  sectionID?: number | null;
  jobTitleID: number;
  employmentStatusID: number;
  profilePhotoPath?: string | null;
  phone?: string;
  birthPlace?: string | null;
  maritalStatus?: string | null;
  militaryNumber?: string | null;
  rankID?: number | null;
  rankName?: string | null;
  militaryServiceDate?: string | null;

  jobPositions?: string | null;
  academicQualifications?: string | null;
  address?: string;
  city?: string | null;
  notes?: string;
  bloodGroup?: string | null;
  nationalIDIssuePlace?: string | null;
  nationalIDIssueDate?: string | null;
  nationalIDExpiryDate?: string | null;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  gender: number;
  hireDate: string;
  birthDate?: string | null;
  isInService: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface HrStatsDto {
  totalEmployees: number;
  pendingLeaveRequests: number;
  employeesOnLeave: number;
}

export interface CreateEmployeeDto {
  departmentID: number;
  sectionID?: number | null;
  jobTitleID: number;
  employmentStatusID: number;
  employeeNumber: string;
  fullName: string;
  nationalID: string;
  phone?: string;
  birthPlace?: string | null;
  maritalStatus?: string | null;
  militaryNumber?: string | null;
  rankID?: number | null;
  militaryServiceDate?: string | null;

  jobPositions?: string | null;
  academicQualifications?: string | null;
  address?: string;
  notes?: string;
  bloodGroup?: string | null;
  nationalIDIssuePlace?: string | null;
  nationalIDIssueDate?: string | null;
  nationalIDExpiryDate?: string | null;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  gender: number;
  hireDate: string;
  birthDate?: string | null;
}

// ─── Leave Requests ──────────────────────────────────────

export interface LeaveRequestStatsDto {
  pendingCount: number;
  approvedThisMonthCount: number;
  totalThisYearCount: number;
}

export interface LeaveRequestListItemDto {
  leaveRequestID: number;
  employeeFullName: string;
  employeeNumber: string;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  requestedDays: number;
  status: string;
  reason?: string;
  notes?: string;
  approvedByUserName?: string;
  createdAt: string;
}

export interface LeaveRequestDetailsDto extends LeaveRequestListItemDto {
  employeeID: number;
  leaveTypeID: number;
  reason?: string;
  notes?: string;
  approvedByUserName?: string;
  approvedAt?: string;
}

export interface CreateLeaveRequestDto {
  employeeID: number;
  leaveTypeID: number;
  startDate: string;
  endDate: string;
  reason?: string;
  notes?: string;
}

export interface ProcessLeaveRequestDto {
  status: "Approved" | "Rejected";
  notes?: string;
}

// ─── Service History ─────────────────────────────────────

export interface ServiceHistoryEventDto {
  serviceHistoryEventID: number;
  employeeID: number;
  employeeFullName: string;
  employeeNumber: string;
  eventType: string;
  eventDate: string;
  summary: string;
  sourceEntityType?: string;
  sourceEntityId?: number;
  createdAt: string;
}

export interface CreateServiceHistoryEventDto {
  employeeID: number;
  eventType: string;
  eventDate: string;
  summary: string;
  sourceEntityType?: string;
  sourceEntityId?: number;
}

// ─── Attendance ─────────────────────────────────────────

export interface AttendanceEventDto {
  attendanceEventID: number;
  employeeID: number;
  employeeFullName: string;
  employeeNumber: string;
  eventDate: string;
  eventType: string;
  eventTime?: string;
  location?: string;
  deviceInfo?: string;
  notes?: string;
  isManualEntry: boolean;
  createdAt: string;
}

export interface CreateAttendanceEventDto {
  employeeID: number;
  eventDate: string;
  eventType: string;
  eventTime?: string;
  location?: string;
  notes?: string;
}

export interface AttendanceSummaryDto {
  employeeID: number;
  employeeFullName: string;
  presentDays: number;
  absentDays: number;
  lateArrivals: number;
  earlyDepartures: number;
  totalWorkHours: number;
}

export interface AttendanceDailyStatsDto {
  date: string;
  totalEmployees: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  externalTaskCount: number;
  presencePercentage: number;
}

// ─── Documents ──────────────────────────────────────────

export interface HrDocumentDto {
  documentID: number;
  documentTypeID: number;
  documentTypeName: string;
  fileName: string;
  filePath?: string;
  fileExtension?: string;
  fileSize?: number;
  notes?: string;
  uploadedByUser?: string;
  createdAt: string;
  issueDate?: string;
  expiryDate?: string;
  documentNumber?: string;
  verified: boolean;
}

export interface HrGlobalDocumentDto {
  documentID: number;
  fileName: string;
  filePath: string;
  fileExtension?: string;
  fileSize: number;
  createdAt: string;
  documentTypeID: number;
  documentTypeName: string;
  uploadedByUser?: string;
  employeeID: number;
  employeeFullName: string;
  employeeNumber: string;
}

// ─── Administrative Orders ──────────────────────────────

export interface CreateAdministrativeOrderDto {
  employeeID: number;
  administrativeOrderTypeID: number;
  orderNumber: string;
  orderDate: string;
  effectiveDate?: string;
  subject: string;
  notes?: string;
}

export interface AdministrativeOrderListItemDto {
  administrativeOrderID: number;
  orderNumber: string;
  typeName: string;
  subject: string;
  employeeFullName: string;
  orderDate: string;
  status: string;
}

// ─── API Methods ─────────────────────────────────────────

export const hrApi = {
  // Reference Data (using the consolidated /hr/HrReferenceData route)
  // Backend returns ApiResponse<T> wrapper — unwrap .data.data to get the actual payload
  getDepartments: () => api.get<ApiResponse<DepartmentDto[]>>("/hr/HrReferenceData/departments").then((r) => r.data.data || []),
  createDepartment: (payload: CreateDepartmentDto) => api.post<ApiResponse<DepartmentDto>>("/hr/HrReferenceData/departments", payload).then((r) => r.data.data),
  updateDepartment: (id: number, payload: Partial<CreateDepartmentDto>) => api.put<ApiResponse<DepartmentDto>>(`/hr/HrReferenceData/departments/${id}`, payload).then((r) => r.data.data),
  
  getSections: (departmentId?: number) =>
    api.get<ApiResponse<SectionDto[]>>("/hr/HrReferenceData/sections", { params: { departmentId } }).then((r) => r.data.data || []),
  createSection: (payload: CreateSectionDto) => api.post<ApiResponse<SectionDto>>("/hr/HrReferenceData/sections", payload).then((r) => r.data.data),
  updateSection: (id: number, payload: Partial<CreateSectionDto>) => api.put<ApiResponse<SectionDto>>(`/hr/HrReferenceData/sections/${id}`, payload).then((r) => r.data.data),

  getJobTitles: () => api.get<ApiResponse<JobTitleDto[]>>("/hr/HrReferenceData/job-titles").then((r) => r.data.data || []),
  createJobTitle: (payload: CreateJobTitleDto) => api.post<ApiResponse<JobTitleDto>>("/hr/HrReferenceData/job-titles", payload).then((r) => r.data.data),
  updateJobTitle: (id: number, payload: Partial<CreateJobTitleDto>) => api.put<ApiResponse<JobTitleDto>>(`/hr/HrReferenceData/job-titles/${id}`, payload).then((r) => r.data.data),
  getEmploymentStatuses: () =>
    api.get<ApiResponse<EmploymentStatusDto[]>>("/hr/HrReferenceData/employment-statuses").then((r) => r.data.data || []),
  getLeaveTypes: () =>
    api.get<ApiResponse<LeaveTypeDto[]>>("/hr/HrReferenceData/leave-types").then((r) => r.data.data || []),
  getRanks: () => api.get<ApiResponse<RankDto[]>>("/hr/HrReferenceData/ranks").then((r) => r.data.data || []),


  // Employees
  getEmployees: (params?: { page?: number; pageSize?: number; search?: string; cursor?: number | null; sortColumn?: string; sortDirection?: "asc" | "desc" | null }) =>
    api.get<ApiResponse<PagedResult<EmployeeListItemDto>>>("/hr/HrEmployees", { params }).then((r) => r.data.data || { items: [], totalCount: 0, page: 1, pageSize: 10 }),
  getEmployeeById: (id: number) =>
    api.get<ApiResponse<EmployeeDetailsDto>>(`/hr/HrEmployees/${id}`).then((r) => r.data.data),
  createEmployee: (payload: CreateEmployeeDto) =>
    api.post<ApiResponse<EmployeeDetailsDto>>("/hr/HrEmployees", payload).then((r) => r.data.data),
  updateEmployee: (id: number, payload: Partial<CreateEmployeeDto>) =>
    api.put<ApiResponse<EmployeeDetailsDto>>(`/hr/HrEmployees/${id}`, payload).then((r) => r.data.data),
  deleteEmployee: (id: number) => api.delete<ApiResponse<boolean>>(`/hr/HrEmployees/${id}`).then((r) => r.data.data),
  uploadEmployeePhoto: (id: number, file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return api.post<ApiResponse<string>>(`/hr/HrEmployees/${id}/photo`, formData, {
      headers: { "Content-Type": "multipart/form-data" },
    }).then((r) => r.data.data);
  },
  getStats: () => api.get<ApiResponse<HrStatsDto>>("/hr/HrEmployees/stats").then((r) => r.data.data || { totalEmployees: 0, pendingLeaveRequests: 0, employeesOnLeave: 0 }),
  getGlobalDocuments: (params?: { page?: number; pageSize?: number; search?: string }) =>
    api.get<ApiResponse<PagedResult<HrGlobalDocumentDto>>>("/hr/HrEmployees/documents/global", { params }).then((r) => r.data.data || { items: [], totalCount: 0, page: 1, pageSize: 10 }),

  // Leave Requests
  getLeaveRequests: (params?: { page?: number; pageSize?: number; employeeId?: number; search?: string; status?: string }) =>
    api.get<ApiResponse<PagedResult<LeaveRequestListItemDto>>>("/hr/HrLeaveRequests", { params }).then((r) => r.data.data || { items: [], totalCount: 0, page: 1, pageSize: 10 }),
  getLeaveStats: () =>
    api.get<ApiResponse<LeaveRequestStatsDto>>("/hr/HrLeaveRequests/stats").then((r) => r.data.data || { pendingCount: 0, approvedThisMonthCount: 0, totalThisYearCount: 0 }),
  getLeaveById: (id: number) =>
    api.get<ApiResponse<LeaveRequestDetailsDto>>(`/hr/HrLeaveRequests/${id}`).then((r) => r.data.data),
  createLeaveRequest: (payload: CreateLeaveRequestDto) =>
    api.post<ApiResponse<LeaveRequestDetailsDto>>("/hr/HrLeaveRequests", payload).then((r) => r.data.data),
  updateLeaveRequest: (id: number, payload: Partial<CreateLeaveRequestDto>) =>
    api.put<ApiResponse<LeaveRequestDetailsDto>>(`/hr/HrLeaveRequests/${id}`, payload).then((r) => r.data.data),
  processLeave: (id: number, payload: ProcessLeaveRequestDto) =>
    api.post<ApiResponse<LeaveRequestDetailsDto>>(`/hr/HrLeaveRequests/${id}/process`, payload).then((r) => r.data.data),

  // Service History
  getGlobalServiceHistory: (params?: { page?: number; pageSize?: number; search?: string }) =>
    api.get<ApiResponse<PagedResult<ServiceHistoryEventDto>>>("/hr/HrServiceHistory", { params }).then((r) => r.data.data || { items: [], totalCount: 0, page: 1, pageSize: 20 }),
  getEmployeeServiceHistory: (employeeId: number) =>
    api.get<ApiResponse<ServiceHistoryEventDto[]>>(`/hr/HrServiceHistory/employee/${employeeId}`).then((r) => r.data.data || []),
  createHistoryEvent: (payload: CreateServiceHistoryEventDto) =>
    api.post<ApiResponse<ServiceHistoryEventDto>>("/hr/HrServiceHistory", payload).then((r) => r.data.data),
  updateHistoryEvent: (id: number, payload: CreateServiceHistoryEventDto) =>
    api.put<ApiResponse<ServiceHistoryEventDto>>(`/hr/HrServiceHistory/${id}`, payload).then((r) => r.data.data),
  deleteHistoryEvent: (id: number) =>
    api.delete<ApiResponse<boolean>>(`/hr/HrServiceHistory/${id}`).then((r) => r.data.data),

  // Attendance
  getAttendance: (params?: { employeeId?: number; fromDate?: string; toDate?: string; page?: number; pageSize?: number }) =>
    api.get<ApiResponse<PagedResult<AttendanceEventDto>>>("/hr/HrAttendance", { params }).then((r) => r.data.data || { items: [], totalCount: 0, page: 1, pageSize: 10 }),
  recordAttendance: (payload: CreateAttendanceEventDto) =>
    api.post<ApiResponse<AttendanceEventDto>>("/hr/HrAttendance", payload).then((r) => r.data.data),
  getEmployeeAttendanceSummary: (employeeId: number, year: number, month: number) =>
    api.get<ApiResponse<AttendanceSummaryDto>>(`/hr/HrAttendance/summary/${employeeId}`, { params: { year, month } }).then((r) => r.data.data || { employeeID: employeeId, employeeFullName: "", presentDays: 0, absentDays: 0, lateArrivals: 0, earlyDepartures: 0, totalWorkHours: 0 }),
  getDailyStats: (date?: string) =>
    api.get<ApiResponse<AttendanceDailyStatsDto>>("/hr/HrAttendance/daily-stats", { params: { date } }).then((r) => r.data.data || { date: date || "", totalEmployees: 0, presentCount: 0, absentCount: 0, lateCount: 0, externalTaskCount: 0, presencePercentage: 0 }),

  // Documents
  getEmployeeDocuments: (employeeId: number) =>
    api.get<ApiResponse<HrDocumentDto[]>>(`/hr/HrDocuments/${employeeId}`).then((r) => r.data.data || []),
  uploadDocument: (payload: FormData) =>
    api.post<ApiResponse<HrDocumentDto>>("/hr/HrDocuments/upload", payload, {
      headers: { "Content-Type": "multipart/form-data" },
    }).then((r) => r.data.data),
  deleteDocument: (id: number) => api.delete<ApiResponse<boolean>>(`/hr/HrDocuments/${id}`).then((r) => r.data.data),

  // Administrative Orders
  getOrders: (params?: { page?: number; pageSize?: number; search?: string }) =>
    api.get<ApiResponse<PagedResult<AdministrativeOrderListItemDto>>>("/hr/HrAdministrativeOrders", { params }).then((r) => r.data.data || { items: [], totalCount: 0, page: 1, pageSize: 10 }),
  createOrder: (payload: CreateAdministrativeOrderDto) =>
    api.post<ApiResponse<AdministrativeOrderListItemDto>>("/hr/HrAdministrativeOrders", payload).then((r) => r.data.data),
  issueOrder: (id: number) => api.post<ApiResponse<boolean>>(`/hr/HrAdministrativeOrders/${id}/issue`).then((r) => r.data.data),
};
