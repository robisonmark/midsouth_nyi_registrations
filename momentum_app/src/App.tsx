import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Provider } from 'react-redux';
import { store } from './store/store';
import SignUpView from './components/SignUpView';
import ScheduleView from './components/ScheduleView';
import BottomNavigation from './components/BottomNavigation';

export default function App() {
  return (
    <Provider store={store}>
      <Router>
        <div className="relative min-h-screen bg-[#fdfdf1] overflow-x-hidden">
          <Routes>
            <Route path="/" element={<Navigate to="/signup/vocal" replace />} />
            <Route path="/signup/:category" element={<SignUpView />} />
            <Route path="/schedule" element={<ScheduleView />} />
            <Route path="*" element={<Navigate to="/signup/vocal" replace />} />
          </Routes>
          
          <BottomNavigation />
        </div>
      </Router>
    </Provider>
  );
}
