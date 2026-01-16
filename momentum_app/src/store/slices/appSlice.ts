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
  headerSubText: String;
}

const initialState: AppState = {
  selectedChurch: "Clarksville Grace",
  selectedCategory: "vocal",
  headerSubText: "Schedule"
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
    setHeaderSubText: (
      state,
      action: PayloadAction<string>,
    ) => {
      state.headerSubText = action.payload;
    }
  },
});

export const { setSelectedChurch, setSelectedCategory, setHeaderSubText } =
  appSlice.actions;
export default appSlice.reducer;