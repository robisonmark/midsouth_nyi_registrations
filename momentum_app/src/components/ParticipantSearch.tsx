import { Dialog, DialogContent, DialogHeader, DialogTitle } from './ui/dialog';
import { Input } from './ui/input';
import React from 'react';

interface Participant {
  id: number;
  name: string;
  church: string;
}

interface ParticipantSearchProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  searchQuery: string;
  setSearchQuery: (q: string) => void;
  searchResults: Participant[];
  isSearching: boolean;
  handleSelectParticipant: (participant: Participant) => void;
}

const ParticipantSearch: React.FC<ParticipantSearchProps> = ({
  isOpen,
  onOpenChange,
  searchQuery,
  setSearchQuery,
  searchResults,
  isSearching,
  handleSelectParticipant,
}) => (
  <Dialog open={isOpen} onOpenChange={onOpenChange}>
    <DialogContent className="w-[calc(100%-2rem)] max-w-md md:max-w-lg">
      <DialogHeader>
        <DialogTitle className="font-['Open_Sans',_sans-serif] text-[#002244] text-[20px] md:text-[24px]">
          Search Participant
        </DialogTitle>
      </DialogHeader>
      <div className="space-y-4 md:space-y-6">
        <div>
          <Input
            type="text"
            placeholder="Type a name to search..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full font-['Inter',_sans-serif] text-[16px] md:text-[18px] h-[44px] md:h-[48px]"
            autoFocus
          />
        </div>
        {/* Search Results */}
        <div className="max-h-[300px] md:max-h-[400px] overflow-y-auto">
          {isSearching ? (
            <div className="text-center py-8 md:py-12 text-[#666] font-['Inter',_sans-serif] text-[16px] md:text-[18px]">
              Searching...
            </div>
          ) : searchQuery.trim().length > 0 ? (
            searchResults.length > 0 ? (
              <div className="space-y-2 md:space-y-3">
                {searchResults.map((participant) => (
                  <button
                    key={participant.id}
                    onClick={() => handleSelectParticipant(participant)}
                    className="w-full text-left px-4 md:px-6 py-3 md:py-4 rounded-lg hover:bg-[#f5f5f5] transition-colors border border-[#e0e0e0]"
                  >
                    <div className="font-['Open_Sans',_sans-serif] text-[16px] md:text-[18px]">
                      <span className="font-semibold text-[#002244]">{participant.name.split(' ')[0]} </span>
                      <span className="font-light text-[#002244]">{participant.name.split(' ')[1]}</span>
                    </div>
                    <div className="text-sm md:text-base text-[#666] font-['Inter',_sans-serif] mt-1">
                      {participant.church}
                    </div>
                  </button>
                ))}
              </div>
            ) : (
              <div className="text-center py-8 md:py-12 text-[#666] font-['Inter',_sans-serif] text-[16px] md:text-[18px]">
                No participants found
              </div>
            )
          ) : (
            <div className="text-center py-8 md:py-12 text-[#666] font-['Inter',_sans-serif] text-[16px] md:text-[18px]">
              Start typing to search for participants
            </div>
          )}
        </div>
      </div>
    </DialogContent>
  </Dialog>
);

export default ParticipantSearch;
