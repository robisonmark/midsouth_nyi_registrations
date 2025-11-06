import { useState, useEffect } from 'react';
import { apiFetch } from '../api/client';
import { useParams } from 'react-router-dom';
import { useAppSelector } from '../store/hooks';
import svgPaths from "../imports/svg-n6pltu4jyi";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from './ui/dialog';
import { Input } from './ui/input';
import type { EventCategory, Church } from '../store/slices/appSlice';

type AgeCategory = 'early-youth' | 'senior-youth';
type EventType = 'Vocal Solo' | 'Sign Language' | 'Preaching' | 'Flag Football' | 'Instrumental Solo' | 'Drama';

interface TimeSlot {
  time: string;
  studentName?: string;
  eventType: EventType;
  location: string;
  church?: Church;
  available: boolean;
}


// Fetch timeslots from API
// API endpoint example: /api/timeslots?category=vocal&age=senior-youth

const students = ['Student One', 'Student Two', 'Dos Estudiante'];

// Mock participant data for API search
const mockParticipants = [
  { id: 1, name: 'Mark Robison', church: 'Hendersonville' },
  { id: 2, name: 'Laura Robison', church: 'Hendersonville' },
  { id: 3, name: 'Laura Aguyao', church: 'Goodlettsville' },
  { id: 4, name: 'Sarah Johnson', church: 'Clarksville Grace' },
  { id: 5, name: 'John Smith', church: 'Hendersonville' },
  { id: 6, name: 'Emma Davis', church: 'Goodlettsville' },
  { id: 7, name: 'Michael Chen', church: 'Clarksville Grace' },
  { id: 8, name: 'Rachel Martinez', church: 'Hendersonville' },
  { id: 9, name: 'David Thompson', church: 'Goodlettsville' },
  { id: 10, name: 'Sophie Anderson', church: 'Clarksville Grace' },
];

// Helper function to get AM/PM indicator
const getTimePeriod = (time: string): string => {
  const hour = parseInt(time.split(':')[0]);
  // Assume 9-11 are AM, 1-8 are PM (typical event schedule)
  if (hour >= 9 && hour <= 11) return 'a';
  return 'p';
};

export default function SignUpView() {
  const { category } = useParams<{ category: string }>();
  const selectedChurch = useAppSelector((state) => state.app.selectedChurch);
  
  // Validate and set the category
  const validCategories: EventCategory[] = ['vocal', 'instrumental', 'speech', 'creative'];
  const selectedCategory: EventCategory = validCategories.includes(category as EventCategory) 
    ? (category as EventCategory) 
    : 'vocal';
  

  const [selectedAgeGroup, setSelectedAgeGroup] = useState<AgeCategory>('senior-youth');
  const [selectedEventType, setSelectedEventType] = useState<string>('');
  const [events, setEvents] = useState<Array<{ id: string; name: string; category: string; type?: string }>>([]);
  const [loadingEvents, setLoadingEvents] = useState(false);
  const [eventsError, setEventsError] = useState<string | null>(null);
  const [filterStudent, setFilterStudent] = useState<string>('all students');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<Array<{ id: number; name: string; church: string }>>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [selectedTimeSlot, setSelectedTimeSlot] = useState<string>('');
  const [timeslots, setTimeslots] = useState<TimeSlot[]>([]);
  const [loadingTimeslots, setLoadingTimeslots] = useState(false);
  const [timeslotError, setTimeslotError] = useState<string | null>(null);


    // Fetch events for the event type select
  useEffect(() => {
    setLoadingEvents(true);
    setEventsError(null);
    apiFetch<Array<{ id: string; name: string; category: string; type?: string }>>('/api/events')
      .then((data) => {
        setEvents(data);
        // Set default event type if not already set
        if (!selectedEventType && data.length > 0) {
          setSelectedEventType(data[0].name);
        }
      })
      .catch((err) => setEventsError(err.message))
      .finally(() => setLoadingEvents(false));
  }, []);

  // Fetch timeslots from API when selected event changes
  useEffect(() => {
    if (!selectedEventType) {
      setTimeslots([]);
      return;
    }
    setLoadingTimeslots(true);
    setTimeslotError(null);
    // Find the selected event object by name
    const selectedEvent = events.find(e => e.name === selectedEventType);
    if (!selectedEvent) {
      setTimeslots([]);
      setLoadingTimeslots(false);
      return;
    }
    apiFetch<TimeSlot[]>(`/api/events/${selectedEvent.id}/time_slots`)
      .then(setTimeslots)
      .catch((err) => setTimeslotError(err.message))
      .finally(() => setLoadingTimeslots(false));
  }, [selectedEventType, events]);

  // Mock API call to search participants
  const searchParticipants = async (query: string) => {
    setIsSearching(true);
    
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // In a real application, this would be an API call like:
    // const response = await fetch(`/api/participants/search?q=${query}`);
    // const data = await response.json();
    
    const results = mockParticipants.filter(participant =>
      participant.name.toLowerCase().includes(query.toLowerCase()) ||
      participant.church.toLowerCase().includes(query.toLowerCase())
    );
    
    setSearchResults(results);
    setIsSearching(false);
  };

  // Debounced search effect
  useEffect(() => {
    if (searchQuery.trim().length > 0) {
      const timer = setTimeout(() => {
        searchParticipants(searchQuery);
      }, 300);
      
      return () => clearTimeout(timer);
    } else {
      setSearchResults([]);
    }
  }, [searchQuery]);

  const handleSignUp = (time: string) => {
    setSelectedTimeSlot(time);
    setIsModalOpen(true);
    setSearchQuery('');
    setSearchResults([]);
  };

  const handleSelectParticipant = (participant: { id: number; name: string; church: string }) => {
    alert(`Signing up ${participant.name} from ${participant.church} for ${selectedTimeSlot} slot!`);
    setIsModalOpen(false);
    setSearchQuery('');
    setSearchResults([]);
  };

  return (
    <>
      {/* Header - Full Width */}
      <header className="relative h-[157px] md:h-[200px] overflow-hidden w-full" style={{ clipPath: 'url(#header-clip)' }}>
        <svg className="absolute inset-0 w-0 h-0">
          <defs>
            <clipPath id="header-clip" clipPathUnits="userSpaceOnUse">
              <path d="M95.5246 106.355C22.3478 106.355 0.684548 140.118 -1 157V0H374.991C374.654 3.37634 374.182 20.2581 374.991 60.7742C375.8 101.29 321.422 108.043 294.133 106.355H95.5246Z" />
            </clipPath>
          </defs>
        </svg>
        
        <div className="absolute inset-0 bg-[#002A48]" />
        
        {/* Header Content Container */}
        <div className="relative max-w-[1400px] mx-auto h-full">
          {/* Momentum Logo */}
          <div className="absolute left-[15px] md:left-[40px] top-[13px] md:top-[20px] w-[158px] md:w-[200px] h-[35px] md:h-[44px]">
            <svg className="block size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 158 35">
              <path clipRule="evenodd" d={svgPaths.p3f704780} fill="#39FF14" fillRule="evenodd" />
            </svg>
          </div>
          
          {/* Taglines */}
          <p className="absolute left-[38px] md:left-[80px] top-[44px] md:top-[60px] text-[#fdfdf1] text-[11px] md:text-[13px] font-['Over_the_Rainbow',_cursive]">From God</p>
          <p className="absolute left-[142px] md:left-[230px] top-[44px] md:top-[60px] text-[#fdfdf1] text-[11px] md:text-[13px] font-['Over_the_Rainbow',_cursive] text-right translate-x-[-100%]">For God</p>
          
          {/* Title */}
          <h1 className="absolute left-[15px] md:left-[40px] top-[63px] md:top-[90px] text-[#fdfdf1] text-[24px] md:text-[36px] font-['Playfair_Display',_serif] lowercase" style={{ fontVariant: 'small-caps' }}>
            {selectedChurch}
          </h1>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-[1400px] mx-auto px-[15px] md:px-[40px] lg:px-[60px] pb-[80px]">
        {/* Controls Section */}
        <div className="mt-[24px] mb-[32px] md:mb-[40px]">
          {/* Event Type Selector */}
          <div className="mb-[16px] md:mb-[20px]">
            {loadingEvents ? (
              <div className="text-[#666] text-center py-2">Loading events...</div>
            ) : eventsError ? (
              <div className="text-red-600 text-center py-2">{eventsError}</div>
            ) : (
              <select
                value={selectedEventType}
                onChange={(e) => setSelectedEventType(e.target.value)}
                className="w-full max-w-[240px] md:max-w-[320px] h-[40px] md:h-[44px] bg-white border border-[#d9d9d9] rounded-[8px] px-[16px] text-[16px] font-['Inter',_sans-serif] mx-auto md:mx-0 block"
              >
                {events.map((event) => (
                  <option key={event.id} value={event.name}>
                    {event.name}
                  </option>
                ))}
              </select>
            )}
          </div>

          {/* Age Group and Filter Row */}
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-[20px]">
            <div className="relative flex items-center justify-between md:justify-start md:gap-8">
              <button 
                onClick={() => setSelectedAgeGroup(selectedAgeGroup === 'senior-youth' ? 'early-youth' : 'senior-youth')}
                className="text-[#002244] text-[24px] md:text-[28px] font-['Open_Sans',_sans-serif] tracking-[-1.92px] flex items-center gap-2"
              >
                {selectedAgeGroup === 'senior-youth' ? 'senior youth' : 'early youth'}
                <svg className="w-[24px] h-[24px] md:w-[28px] md:h-[28px]" fill="none" viewBox="0 0 24 24">
                  <path d={svgPaths.p2b1b0180} fill="#1D1B20" />
                </svg>
              </button>
              
              <div className="w-[14px] h-[14px] md:hidden">
                <svg className="block size-full" fill="none" viewBox="0 0 14 14">
                  <path d={svgPaths.p3f43b940} stroke="#1E1E1E" strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.2" />
                </svg>
              </div>
            </div>

            {/* Student Filter */}
            <div>
              <select 
                value={filterStudent}
                onChange={(e) => setFilterStudent(e.target.value)}
                className="w-full md:w-[240px] h-[40px] md:h-[44px] bg-white border border-[#d9d9d9] rounded-[8px] px-[16px] text-[16px] font-['Inter',_sans-serif]"
              >
                <option>all students</option>
                {students.map(student => (
                  <option key={student}>{student}</option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {/* Time Slots */}
        <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-8 md:gap-y-10">
          {loadingTimeslots ? (
            <div className="col-span-full text-center py-8 text-[#666]">Loading timeslots...</div>
          ) : timeslotError ? (
            <div className="col-span-full text-center py-8 text-red-600">{timeslotError}</div>
          ) : timeslots.length === 0 ? (
            <div className="col-span-full text-center py-8 text-[#666]">No timeslots available.</div>
          ) : (
            timeslots.map((slot, index) => (
              <div key={index} className="pb-[24px] md:pb-[32px] border-b border-[#2A323F]/30">
                <div className="flex items-start justify-between gap-4">
                  {/* Time */}
                  <div className="text-[#002244] text-[48px] md:text-[56px] font-['Playfair_Display_SC',_serif] leading-none flex items-start gap-1 shrink-0">
                    <span>{slot.time}</span>
                    <span className="text-[24px] md:text-[28px] font-['Open_Sans',_sans-serif] pt-1">{getTimePeriod(slot.time)}</span>
                  </div>

                  {/* Student Info or Sign Up Button */}
                  <div className="flex-1 min-w-0">
                    {slot.available ? (
                      <button
                        onClick={() => handleSignUp(slot.time)}
                        className="bg-[#6abf28] text-[#2a323f] text-[16px] md:text-[18px] font-['Open_Sans',_sans-serif] px-[24px] md:px-[32px] py-[12px] md:py-[14px] rounded lowercase hover:bg-[#5da622] transition-colors"
                      >
                        Sign Up
                      </button>
                    ) : (
                      <div>
                        <div className="font-['Open_Sans',_sans-serif] text-[16px] md:text-[18px]">
                          <span className="font-semibold">{slot.studentName?.split(' ')[0]} </span>
                          <span className="font-light">{slot.studentName?.split(' ')[1]}</span>
                        </div>
                        <div className="text-[#002244] text-[16px] md:text-[18px] font-['Open_Sans',_sans-serif] font-light">
                          {slot.church}
                        </div>
                        <div className="text-[#666] text-[14px] md:text-[15px] font-['Open_Sans',_sans-serif] font-light mt-1">
                          {slot.location}
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </main>

      {/* Participant Search Modal */}
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
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
    </>
  );
}
