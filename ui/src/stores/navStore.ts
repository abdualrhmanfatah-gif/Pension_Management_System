import { create } from "zustand";

interface NavState {
  hasSecondaryNav: boolean;
  setHasSecondaryNav: (v: boolean) => void;
}

export const useNavStore = create<NavState>((set) => ({
  hasSecondaryNav: false,
  setHasSecondaryNav: (v) => set({ hasSecondaryNav: v }),
}));
