import { CorrespondenceDirection } from "@/types/correspondence";

import { CorrespondenceRegisterPage } from "./CorrespondenceRegisterPage";

export function OutgoingRegisterPage() {
  return <CorrespondenceRegisterPage direction={CorrespondenceDirection.Outgoing} />;
}
