import axios from 'axios';

// const API_BASE_URL = 'http://localhost:5159'; // עדכן את ה-port לפי הצורך
// axios.defaults.baseURL = API_BASE_URL;

// const apiUrl = "http://localhost:5159"

const apiUrl = process.env.REACT_APP_API_URL;

axios.defaults.baseURL = "https://"+apiUrl;  // הגדרת baseURL לפי משתנה הסביבה או URL ברירת המחדל
console.log("API Base URL:", apiUrl);
axios.defaults.headers.common['Content-Type'] = 'application/json';

axios.interceptors.response.use(
  response => response,
  error => {
      console.error('API Error:', error); // רושם את השגיאה ללוג
      return Promise.reject(error);
  }
);

export default {
  getTasks: async () => {
    const result = await axios.get(`/items`)    
    return result.data;
  },

  addTask: async(name)=>{
    console.log('addTask', name)
    const result = await axios.post(`/items`,{name})
    return result.data;
  },

  setCompleted: async(id, isComplete)=>{
    console.log('setCompleted', {id, isComplete})
    const result = await axios.put(`/items/${id}`,{isComplete})
    return result.data;
  },

  deleteTask: async(id)=>{
    console.log('deleteTask')
    await axios.delete(`/items/${id}`)
    return {};
  }
};
