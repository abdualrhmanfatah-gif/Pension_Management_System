import { CorrespondenceDirection } from "@/types/correspondence";

import { CorrespondenceRegisterPage } from "./CorrespondenceRegisterPage";

export function IncomingRegisterPage() {
  return <CorrespondenceRegisterPage direction={CorrespondenceDirection.Incoming} />;
}
