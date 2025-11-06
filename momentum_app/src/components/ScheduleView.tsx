import { useState } from 'react';
import { useAppSelector } from '../store/hooks';
import svgPaths from "../imports/svg-n6pltu4jyi";
import type { EventCategory, Church } from '../store/slices/appSlice';

type EventType = 'Vocal Solo' | 'Sign Language' | 'Preaching' | 'Flag Football' | 'Instrumental Solo' | 'Drama';

interface ScheduledEvent {
  time: string;
  studentName: string;
  eventType: EventType;
  location: string;
  church: Church;
  category: EventCategory;
}

// Mock scheduled events for the group
const mockScheduledEvents: ScheduledEvent[] = [
    { time: '9:00', studentName: 'Student One', eventType: 'Preaching', location: 'The Well', church: 'Clarksville Grace', category: 'speech' },
    { time: '10:15', studentName: 'Student One', eventType: 'Vocal Solo', location: 'The Well', church: 'Clarksville Grace', category: 'vocal' },
  { time: '1:20', studentName: 'Dos Estudiante', eventType: 'Preaching', location: 'Sanctuary', church: 'Clarksville Grace', category: 'speech' },
  { time: '1:30', studentName: 'Student Two', eventType: 'Vocal Solo', location: 'The Well', church: 'Clarksville Grace', category: 'vocal' },
  { time: '2:00', studentName: 'Student One', eventType: 'Drama', location: 'The Mount', church: 'Clarksville Grace', category: 'creative' },
  { time: '3:15', studentName: 'Dos Estudiante', eventType: 'Instrumental Solo', location: 'Sanctuary', church: 'Clarksville Grace', category: 'instrumental' },
];

const students = ['All Students', 'Student One', 'Student Two', 'Dos Estudiante'];

// Helper function to get AM/PM indicator
const getTimePeriod = (time: string): string => {
  const hour = parseInt(time.split(':')[0]);
  // Assume 9-11 are AM, 1-8 are PM (typical event schedule)
  if (hour >= 9 && hour <= 11) return 'a';
  return 'p';
};

export default function ScheduleView() {
  const selectedChurch = useAppSelector((state) => state.app.selectedChurch);
  const [selectedStudent, setSelectedStudent] = useState<string>('All Students');

  // Filter events by selected student
  const filteredEvents = selectedStudent === 'All Students' 
    ? mockScheduledEvents 
    : mockScheduledEvents.filter(event => event.studentName === selectedStudent);

  // Sort events by time
  const sortedEvents = [...filteredEvents].sort((a, b) => {
    const timeA = parseFloat(a.time.replace(':', '.'));
    const timeB = parseFloat(b.time.replace(':', '.'));
    return timeA - timeB;
  });

  return (
    <>
      {/* Header - Full Width */}
      <header className="relative h-[157px] md:h-[200px] overflow-hidden w-full" style={{ clipPath: 'url(#header-clip-schedule)' }}>
        <svg className="absolute inset-0 w-0 h-0">
          <defs>
            <clipPath id="header-clip-schedule" clipPathUnits="userSpaceOnUse">
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
        {/* Page Title and Filter Row */}
        <div className="mt-[24px] md:mt-[32px] mb-[32px] md:mb-[40px]">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 md:gap-6">
            <h2 className="text-[#002244] text-[32px] md:text-[40px] lg:text-[48px] font-['Playfair_Display',_serif]">
              Team Schedule
            </h2>
            
            {/* Student Filter */}
            <select 
              value={selectedStudent}
              onChange={(e) => setSelectedStudent(e.target.value)}
              className="w-full md:w-[240px] lg:w-[280px] h-[40px] md:h-[44px] bg-white border border-[#d9d9d9] rounded-[8px] px-[16px] text-[16px] font-['Inter',_sans-serif]"
            >
              {students.map(student => (
                <option key={student}>{student}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Schedule List */}
        {sortedEvents.length > 0 ? (
          <div className="space-y-[20px] md:space-y-[24px]">
            {sortedEvents.map((event, index) => (
              <div key={index} className="flex flex-col sm:flex-row sm:items-start gap-[12px] sm:gap-[16px] md:gap-[24px] pb-[20px] md:pb-[24px] border-b border-[#2A323F]/20 last:border-b-0">
                {/* Time Column */}
                <div className="text-[#002244] text-[48px] md:text-[56px] lg:text-[64px] font-['Playfair_Display_SC',_serif] leading-none sm:min-w-[80px] md:min-w-[100px] lg:min-w-[120px] flex items-start gap-1">
                  <span>{event.time}</span>
                  <span className="text-[24px] md:text-[28px] lg:text-[32px] font-['Open_Sans',_sans-serif] pt-1">{getTimePeriod(event.time)}</span>
                </div>
                
                {/* Student & Event Column */}
                <div className="flex-1 pt-[4px]">
                  {/* Student Name */}
                  <div className="font-['Open_Sans',_sans-serif] text-[16px] md:text-[18px] lg:text-[20px]">
                    <span className="font-semibold">{event.studentName.split(' ')[0]} </span>
                    <span className="font-light">{event.studentName.split(' ').slice(1).join(' ')}</span>
                  </div>
                  
                  {/* Event Type */}
                  <div className="text-[#002244] text-[16px] md:text-[18px] lg:text-[20px] font-['Open_Sans',_sans-serif] font-light mt-[2px]">
                    {event.eventType}
                  </div>
                </div>
                
                {/* Location Column */}
                <div className="text-[#002244] text-[16px] md:text-[18px] lg:text-[20px] font-['Open_Sans',_sans-serif] font-light pt-[4px] sm:min-w-[100px] md:min-w-[120px] sm:text-right">
                  {event.location}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-[48px] md:py-[64px]">
            <p className="text-[#002244] text-[18px] md:text-[20px] font-['Open_Sans',_sans-serif] font-light">
              No scheduled events
            </p>
          </div>
        )}
      </main>
    </>
  );
}
