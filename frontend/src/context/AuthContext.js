

import React, { createContext, useState, useEffect, useContext } from 'react';
import api from '../services/api'; 


const AuthContext = createContext({});


export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true); 

    
    useEffect(() => {
        const loadUserFromStorage = async () => {
            const token = localStorage.getItem('authToken');
            if (token) {
                try {
                    api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
                    const response = await api.get('/api/users/me'); 
                    setUser(response.data); 
                } catch (error) {
                    
                    localStorage.removeItem('authToken');
                    api.defaults.headers.common['Authorization'] = null;
                    console.error("Erro ao carregar usuário pelo token:", error);
                }
            }
            setLoading(false); 
        };
        loadUserFromStorage();
    }, []); 

    
    const login = async (email, password) => {
        
        try {
            const response = await api.post('/login', { email, password });
            const { accessToken } = response.data;
            
            localStorage.setItem('authToken', accessToken);
            api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;

            const userResponse = await api.get('/api/users/me');
            setUser(userResponse.data); 
            return userResponse.data; 
        } catch (error) {
            
            localStorage.removeItem('authToken');
            api.defaults.headers.common['Authorization'] = null;
            setUser(null);
            throw error; 
        }
    };

    
    const logout = () => {
        localStorage.removeItem('authToken');
        api.defaults.headers.common['Authorization'] = null;
        setUser(null); 
    };

    
    return (
        <AuthContext.Provider value={{ isAuthenticated: !!user, user, login, logout, loading }}>
            {!loading && children} {}
        </AuthContext.Provider>
    );
};


export const useAuth = () => {
    return useContext(AuthContext);
};