import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export type EventCategory =
  | "instrumental"
  | "vocal"
  | "speech"
  | "creative";
export type Church =
  | "Clarksville Grace"
  | "Hendersonville"
  | "Goodlettsville"
  | "Gallatin";

interface AppState {
  selectedChurch: Church;
  selectedCategory: EventCategory;
}

const initialState: AppState = {
  selectedChurch: "Clarksville Grace",
  selectedCategory: "vocal",
};

const appSlice = createSlice({
  name: "app",
  initialState,
  reducers: {
    setSelectedChurch: (
      state,
      action: PayloadAction<Church>,
    ) => {
      state.selectedChurch = action.payload;
    },
    setSelectedCategory: (
      state,
      action: PayloadAction<EventCategory>,
    ) => {
      state.selectedCategory = action.payload;
    },
  },
});

export const { setSelectedChurch, setSelectedCategory } =
  appSlice.actions;
export default appSlice.reducer;